"""
Import Vortex_Base.glb / Vortex_Base.fbx from production/Vortex/source/
into Vortex_Production.blend.

Pipeline:
1. Detect source file
2. Import into the production blend
3. Validate scale / orientation
4. Fit height to 2.05 m
5. Apply transforms
6. Organize into collections
7. Validate materials
8. Validate rig
9. Write correction report
10. Never replace approved art
11. Never export to Unity until visual approval

Usage:
  blender -b Vortex_Production.blend --python import_vortex_base_model.py

Optional env:
  VORTEX_FORCE_IMPORT=1  — replace previous base import (still never touches approved art)
"""

from __future__ import annotations

import os
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import bpy
from mathutils import Vector

from vortex_common import (
    BASE_MODEL_CANDIDATES,
    BLEND_PATH,
    HEIGHT_REF_NAME,
    HUMANOID_BONES_MIN,
    IMPORT_TAG_BASE,
    PROP_APPROVED_ART,
    PROP_BASE_IMPORTED,
    PROP_BASE_SOURCE_FILE,
    PROP_IMPORT_TAG,
    PROP_UNITY_EXPORT_ALLOWED,
    PROP_VISUAL_APPROVED,
    REPORTS_DIR,
    REQUIRED_MATERIALS,
    RIG_OBJECT_NAME,
    SOURCE_DIR,
    TARGET_HEIGHT_M,
    TECHNICAL_OBJECT_NAMES,
)
from vortex_report import Report
from validate_materials_textures import validate_materials_textures
from validate_humanoid_rig import validate_humanoid_rig
from validate_scale_pivot import validate_scale_pivot
from validate_sockets import validate_sockets


TECHNICAL_KEEP = set(TECHNICAL_OBJECT_NAMES) | {HEIGHT_REF_NAME}


def find_base_model(source_dir: Path = SOURCE_DIR) -> Path | None:
    source_dir.mkdir(parents=True, exist_ok=True)
    for name in BASE_MODEL_CANDIDATES:
        path = source_dir / name
        if path.is_file():
            return path
    # Case-insensitive fallback
    lower_map = {p.name.lower(): p for p in source_dir.iterdir() if p.is_file()}
    for name in BASE_MODEL_CANDIDATES:
        hit = lower_map.get(name.lower())
        if hit is not None:
            return hit
    return None


def _ensure_collection(name: str):
    col = bpy.data.collections.get(name)
    if col is None:
        col = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(col)
    return col


def _unlink_from_all_collections(obj):
    for col in list(obj.users_collection):
        col.objects.unlink(obj)


def _link_only(obj, collection_name: str):
    col = _ensure_collection(collection_name)
    _unlink_from_all_collections(obj)
    if obj.name not in col.objects:
        col.objects.link(obj)


def _is_approved(obj) -> bool:
    return bool(obj.get(PROP_APPROVED_ART, False))


def _is_previous_base_import(obj) -> bool:
    return obj.get(PROP_IMPORT_TAG) == IMPORT_TAG_BASE


def _scene_has_approved_art() -> bool:
    return any(_is_approved(o) for o in bpy.data.objects)


def _world_mesh_bounds(objects) -> tuple[Vector, Vector] | None:
    coords = []
    for obj in objects:
        if obj.type != "MESH" or obj.data is None or len(obj.data.vertices) == 0:
            continue
        coords.extend(obj.matrix_world @ v.co for v in obj.data.vertices)
    if not coords:
        return None
    xs = [v.x for v in coords]
    ys = [v.y for v in coords]
    zs = [v.z for v in coords]
    return Vector((min(xs), min(ys), min(zs))), Vector((max(xs), max(ys), max(zs)))


def _deselect_all():
    bpy.ops.object.select_all(action="DESELECT")


def _select_objects(objects, active=None):
    _deselect_all()
    for obj in objects:
        obj.select_set(True)
    if active is not None:
        bpy.context.view_layer.objects.active = active
    elif objects:
        bpy.context.view_layer.objects.active = objects[0]


def remove_previous_base_import(force: bool, report: Report) -> int:
    """Remove prior vortex_base import objects. Never deletes approved art."""
    removed = 0
    victims = [o for o in list(bpy.data.objects) if _is_previous_base_import(o)]
    if not victims:
        report.add("Previous base import cleanup", True, "none to remove")
        return 0

    approved_blocked = [o.name for o in victims if _is_approved(o)]
    if approved_blocked and not force:
        report.add(
            "Refusing to remove approved art",
            False,
            f"approved objects tagged as base: {approved_blocked}",
        )
        return 0

    if _scene_has_approved_art() and not force:
        # Extra safety: if any approved art exists, only remove non-approved previous imports
        victims = [o for o in victims if not _is_approved(o)]

    for obj in victims:
        if _is_approved(obj):
            report.add(
                f"Keep approved `{obj.name}`",
                True,
                "VALGOR_APPROVED_ART=True — not replaced",
            )
            continue
        bpy.data.objects.remove(obj, do_unlink=True)
        removed += 1

    report.add("Removed previous base import objects", True, f"count={removed}")
    return removed


def import_file(path: Path, report: Report) -> list:
    """Import GLB/GLTF/FBX and return newly created objects."""
    before = set(bpy.data.objects)
    suffix = path.suffix.lower()
    filepath = str(path)

    if suffix in {".glb", ".gltf"}:
        bpy.ops.import_scene.gltf(filepath=filepath)
    elif suffix == ".fbx":
        bpy.ops.import_scene.fbx(
            filepath=filepath,
            automatic_bone_orientation=True,
            use_image_search=True,
        )
    else:
        report.add("Supported format", False, f"unsupported suffix {suffix}")
        return []

    after = [o for o in bpy.data.objects if o not in before]
    report.add(
        f"Imported `{path.name}`",
        len(after) > 0,
        f"new_objects={len(after)} types={sorted({o.type for o in after})}",
    )
    return after


def tag_imported(objects, source_path: Path):
    for obj in objects:
        obj[PROP_IMPORT_TAG] = IMPORT_TAG_BASE
        obj[PROP_APPROVED_ART] = False
        obj["VALGOR_SOURCE_NAME"] = source_path.name


def fit_height_and_feet(objects, report: Report) -> None:
    meshes = [o for o in objects if o.type == "MESH"]
    roots = objects
    bounds = _world_mesh_bounds(meshes)
    if bounds is None:
        report.add("Mesh bounds for scale fit", False, "no mesh vertices in import")
        return

    min_c, max_c = bounds
    height = max_c.z - min_c.z
    report.add(
        "Measured import height (pre-fit)",
        height > 0.01,
        f"height={height:.4f} m min_z={min_c.z:.4f} max_z={max_c.z:.4f}",
    )

    # Parent-aware: scale top-level imported objects only
    top_level = [o for o in roots if o.parent is None or o.parent not in roots]
    if height > 1e-6:
        scale_factor = TARGET_HEIGHT_M / height
        for obj in top_level:
            obj.scale *= scale_factor
        # Update matrices
        bpy.context.view_layer.update()
        report.add(
            "Scaled to target height 2.05 m",
            True,
            f"scale_factor={scale_factor:.6f}",
        )
    else:
        report.add("Scaled to target height 2.05 m", False, "height too small")

    bounds2 = _world_mesh_bounds(meshes)
    if bounds2 is None:
        return
    min2, max2 = bounds2
    dz = -min2.z
    dx = -((min2.x + max2.x) * 0.5)
    dy = -((min2.y + max2.y) * 0.5)
    for obj in top_level:
        obj.location.x += dx
        obj.location.y += dy
        obj.location.z += dz
    bpy.context.view_layer.update()

    bounds3 = _world_mesh_bounds(meshes)
    if bounds3:
        min3, max3 = bounds3
        h3 = max3.z - min3.z
        report.add(
            "Post-fit height ≈ 2.05 m",
            abs(h3 - TARGET_HEIGHT_M) <= 0.05,
            f"height={h3:.4f} min_z={min3.z:.4f}",
        )
        report.add(
            "Feet near Z=0 / centered XZ",
            abs(min3.z) <= 0.05 and abs((min3.x + max3.x) * 0.5) <= 0.05,
            f"min_z={min3.z:.4f} center_x={(min3.x + max3.x) * 0.5:.4f}",
            level="warn",
        )


def apply_transforms(objects, report: Report) -> None:
    targets = [o for o in objects if o.type in {"MESH", "ARMATURE", "EMPTY"} and o.name in bpy.data.objects]
    if not targets:
        report.add("Apply transforms", False, "no targets")
        return
    _select_objects(targets)
    try:
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        report.add("Apply transforms", True, f"objects={len(targets)}")
    except Exception as ex:  # noqa: BLE001
        report.add("Apply transforms", False, str(ex), level="warn")


def organize_collections(objects, report: Report) -> None:
    model_col = _ensure_collection("VORTEX_MODEL")
    lod0 = _ensure_collection("VORTEX_LOD0")
    armor = _ensure_collection("VORTEX_ARMOR")
    cape = _ensure_collection("VORTEX_CAPE")
    weapon = _ensure_collection("VORTEX_WEAPON")
    rig_col = _ensure_collection("VORTEX_RIG")

    counts = {"MODEL": 0, "LOD0": 0, "RIG": 0, "ARMOR": 0, "CAPE": 0, "WEAPON": 0}

    for obj in objects:
        if obj.name not in bpy.data.objects:
            continue
        lname = obj.name.lower()
        if obj.type == "ARMATURE":
            _link_only(obj, "VORTEX_RIG")
            # Prefer renaming provisional placeholder if this is the real imported rig
            if RIG_OBJECT_NAME in bpy.data.objects and obj.name != RIG_OBJECT_NAME:
                placeholder = bpy.data.objects.get(RIG_OBJECT_NAME)
                if placeholder is not None and placeholder.get(PROP_IMPORT_TAG) != IMPORT_TAG_BASE:
                    # Keep placeholder; rename imported to Vortex_Rig_Imported
                    obj.name = "Vortex_Rig_Imported"
            counts["RIG"] += 1
            continue

        if obj.type == "MESH":
            _link_only(obj, "VORTEX_MODEL")
            # Also instance into LOD0 as the base hero mesh
            if obj.name not in lod0.objects:
                lod0.objects.link(obj)
            counts["MODEL"] += 1
            counts["LOD0"] += 1

            if any(k in lname for k in ("armor", "armadura", "plate", "helmet", "ombro")):
                if obj.name not in armor.objects:
                    armor.objects.link(obj)
                counts["ARMOR"] += 1
            if any(k in lname for k in ("cape", "cloak", "capa")):
                if obj.name not in cape.objects:
                    cape.objects.link(obj)
                counts["CAPE"] += 1
            if any(k in lname for k in ("sword", "weapon", "espada", "blade")):
                if obj.name not in weapon.objects:
                    weapon.objects.link(obj)
                counts["WEAPON"] += 1
            continue

        # Empties / other → keep under MODEL unless socket-like
        if obj.name.startswith("Socket_"):
            _link_only(obj, "VORTEX_SOCKETS")
        else:
            _link_only(obj, "VORTEX_MODEL")
            counts["MODEL"] += 1

    report.add(
        "Organized into collections",
        counts["MODEL"] > 0 or counts["RIG"] > 0,
        str(counts),
    )
    # Silence unused if empty collections referenced
    _ = model_col


def validate_orientation(objects, report: Report) -> None:
    meshes = [o for o in objects if o.type == "MESH"]
    bounds = _world_mesh_bounds(meshes)
    if bounds is None:
        report.add("Orientation / upright", False, "no mesh")
        return
    min_c, max_c = bounds
    hx = max_c.x - min_c.x
    hy = max_c.y - min_c.y
    hz = max_c.z - min_c.z
    upright = hz >= hx and hz >= hy
    report.add(
        "Character upright on Z (Blender up)",
        upright,
        f"extents=({hx:.3f}, {hy:.3f}, {hz:.3f})",
        level="warn" if not upright else "info",
    )
    report.notes.append(
        "Convenção Valgor/Unity: Up=+Y, Forward=+Z na exportação FBX (-Z Forward, Y Up). "
        "Na cena Blender o eixo vertical é Z."
    )


def set_scene_gates(source_path: Path):
    scene = bpy.context.scene
    scene[PROP_BASE_IMPORTED] = True
    scene[PROP_BASE_SOURCE_FILE] = source_path.name
    scene[PROP_VISUAL_APPROVED] = False
    scene[PROP_UNITY_EXPORT_ALLOWED] = False
    scene["VALGOR_CHARACTER_ID"] = "HERO_VORTEX_000"
    scene["VALGOR_HEIGHT_M"] = TARGET_HEIGHT_M


def build_correction_report(
    import_report: Report,
    imported: list,
) -> Report:
    report = Report("Vortex — Base Import Correction Report")
    report.notes.append("Import summary:")
    for c in import_report.checks:
        report.checks.append(c)
    report.notes.extend(import_report.notes)

    # Materials / rig / scale / sockets after import
    report.extend(validate_scale_pivot(Report("scale")).checks)
    report.extend(validate_humanoid_rig(Report("rig")).checks)
    report.extend(validate_materials_textures(Report("mats")).checks)
    report.extend(validate_sockets(Report("sockets")).checks)

    meshes = [o for o in imported if o.type == "MESH" and o.name in bpy.data.objects]
    armatures = [o for o in imported if o.type == "ARMATURE" and o.name in bpy.data.objects]

    if not meshes:
        report.notes.append("CORRIGIR: nenhuma malha artística importada.")
    if not armatures:
        report.notes.append(
            "CORRIGIR: sem armature no arquivo base — criar/substituir Vortex_Rig Humanoid completo."
        )
    else:
        bones = {b.name for b in armatures[0].data.bones}
        missing = [b for b in HUMANOID_BONES_MIN if b not in bones]
        if missing:
            report.notes.append(
                f"CORRIGIR rig Humanoid: faltam {len(missing)} ossos "
                f"({', '.join(missing[:10])}{'…' if len(missing) > 10 else ''})."
            )

    present_mats = {m.name for m in bpy.data.materials}
    missing_mats = [m for m in REQUIRED_MATERIALS if m not in present_mats]
    if missing_mats:
        report.notes.append("CORRIGIR materiais ausentes: " + ", ".join(missing_mats))
    else:
        report.notes.append(
            "Materiais placeholder presentes; mapear texturas reais PBR nos slots MAT_Vortex_*."
        )

    report.notes.extend(
        [
            "CORRIGIR / completar após import do base:",
            "- Conferir silhueta vs prancha aprovada (rosto, armadura, capa, espada).",
            "- Garantir A-pose/T-pose e skinning limpo.",
            "- Separar submeshes em VORTEX_ARMOR / CAPE / WEAPON se ainda estiverem juntos.",
            "- Gerar LOD1/LOD2 (coleções ainda podem estar vazias).",
            "- Produzir texturas listadas no brief.",
            "- Marcar aprovação visual somente após review (não exportar Unity antes).",
            "Gate atual: VALGOR_VISUAL_APPROVED=False, VALGOR_UNITY_EXPORT_ALLOWED=False.",
        ]
    )
    return report


def import_vortex_base_model(force: bool | None = None) -> Report:
    if force is None:
        force = os.environ.get("VORTEX_FORCE_IMPORT", "0") == "1"

    report = Report("Vortex — Import Base Model")
    SOURCE_DIR.mkdir(parents=True, exist_ok=True)
    REPORTS_DIR.mkdir(parents=True, exist_ok=True)

    source = find_base_model()
    if source is None:
        report.add(
            "Source model present",
            False,
            f"Place Vortex_Base.glb or Vortex_Base.fbx in {SOURCE_DIR}",
            level="warn",
        )
        report.notes.append("Aguardando arquivo em source/. Nenhum import executado.")
        report.write(REPORTS_DIR / "09_import_base_model.txt")
        return report

    report.add("Source model present", True, str(source))

    if _scene_has_approved_art() and not force:
        report.add(
            "Approved art protection",
            True,
            "scene has VALGOR_APPROVED_ART objects — previous approved meshes will not be replaced",
        )

    remove_previous_base_import(force=force, report=report)

    imported = import_file(source, report)
    if not imported:
        report.write(REPORTS_DIR / "09_import_base_model.txt")
        return report

    # Drop accidental import of technical name collisions
    imported = [o for o in imported if o.name not in TECHNICAL_KEEP or _is_previous_base_import(o)]

    tag_imported(imported, source)
    fit_height_and_feet(imported, report)
    apply_transforms(imported, report)
    validate_orientation(imported, report)
    organize_collections(imported, report)
    set_scene_gates(source)

    report.add(
        "Unity export gated until visual approval",
        bpy.context.scene.get(PROP_UNITY_EXPORT_ALLOWED) is False
        and bpy.context.scene.get(PROP_VISUAL_APPROVED) is False,
        "VALGOR_UNITY_EXPORT_ALLOWED=False",
    )

    correction = build_correction_report(report, imported)
    correction.write(REPORTS_DIR / "10_base_import_corrections.txt")
    report.write(REPORTS_DIR / "09_import_base_model.txt")

    # Auto proportion pass after base import (heroic 8.25 heads / 2.05 m)
    try:
        from fix_vortex_proportions import main as fix_proportions_main

        fix_proportions_main()
        report.notes.append("Ran fix_vortex_proportions.py after import.")
    except Exception as ex:  # noqa: BLE001
        report.notes.append(f"Proportion auto-fix skipped: {ex}")

    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    report.notes.append(f"Saved blend: {BLEND_PATH}")
    report.notes.append(f"Correction report: {REPORTS_DIR / '10_base_import_corrections.txt'}")
    return report


def main() -> int:
    report = import_vortex_base_model()
    print(report.to_text())
    # Missing source is a soft wait (exit 0) so watchers can keep polling
    if any(c.name == "Source model present" and not c.ok for c in report.checks):
        print("WAITING_FOR_SOURCE")
        return 0
    return 0 if report.failed == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
