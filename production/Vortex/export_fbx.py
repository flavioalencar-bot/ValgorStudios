"""6) Export FBX for Unity — refuses to export technical primitives as final hero art."""

from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import bpy

from vortex_common import (
    EXPORT_DIR,
    TECHNICAL_OBJECT_NAMES,
    HEIGHT_REF_NAME,
    GROUND_NAME,
    RIG_OBJECT_NAME,
    PROP_UNITY_EXPORT_ALLOWED,
    PROP_VISUAL_APPROVED,
)
from vortex_report import Report


def _is_technical(obj) -> bool:
    if obj.name in TECHNICAL_OBJECT_NAMES:
        return True
    if obj.name.startswith("Socket_"):
        return True
    if obj.name.startswith("Vortex_HeroicProportionGuide"):
        return True
    if obj.name.startswith("VortexPreview"):
        return True
    if obj.name in {HEIGHT_REF_NAME, GROUND_NAME, "VortexRearRimLight"}:
        return True
    if obj.get("VALGOR_IMPORT_TAG") == "proportion_guide":
        return True
    return False


def _hero_mesh_candidates():
    meshes = []
    for obj in bpy.data.objects:
        if obj.type != "MESH":
            continue
        if _is_technical(obj):
            continue
        meshes.append(obj)
    # Prefer explicit Vortex_Base first
    meshes.sort(key=lambda o: (0 if o.name == "Vortex_Base" else 1, o.name))
    return meshes


def _count_tris(obj) -> int:
    mesh = obj.data
    mesh.calc_loop_triangles()
    return len(mesh.loop_triangles)


def export_fbx(force_technical: bool = False, report: Report | None = None) -> Report:
    """
    Exports Vortex_LOD*.fbx only when real hero meshes exist.
    Never promotes height-ref cubes / ground plane as final art.
    """
    report = report or Report("Vortex — Export FBX")
    EXPORT_DIR.mkdir(parents=True, exist_ok=True)

    scene = bpy.context.scene
    visual_ok = bool(scene.get(PROP_VISUAL_APPROVED, False))
    export_ok = bool(scene.get(PROP_UNITY_EXPORT_ALLOWED, False))
    report.add(
        "Visual approval gate",
        visual_ok and export_ok,
        (
            f"{PROP_VISUAL_APPROVED}={visual_ok} {PROP_UNITY_EXPORT_ALLOWED}={export_ok}"
            if not (visual_ok and export_ok)
            else "approved"
        ),
        level="warn" if not (visual_ok and export_ok) else "info",
    )
    if not (visual_ok and export_ok):
        report.notes.append(
            "Export Unity bloqueado até aprovação visual. "
            "Use mark_visual_approved.py somente após review."
        )
        return report

    hero_meshes = _hero_mesh_candidates()
    report.add(
        "Artistic hero mesh available for export",
        len(hero_meshes) > 0,
        (
            f"{len(hero_meshes)} mesh(es): {', '.join(o.name for o in hero_meshes)}"
            if hero_meshes
            else "only technical references present — export blocked (expected until art exists)"
        ),
        level="warn" if not hero_meshes else "info",
    )

    if not hero_meshes and not force_technical:
        report.notes.append(
            "Exportação FBX bloqueada de propósito: não criar modelo final a partir de cubos/primitivas técnicas."
        )
        report.notes.append(
            "Quando o modelo real existir nas coleções VORTEX_LOD0/1/2 (ou VORTEX_MODEL), rode este script de novo."
        )
        return report

    # Group by LOD collection membership
    lod_map = {
        "LOD0": bpy.data.collections.get("VORTEX_LOD0"),
        "LOD1": bpy.data.collections.get("VORTEX_LOD1"),
        "LOD2": bpy.data.collections.get("VORTEX_LOD2"),
    }

    armature = bpy.data.objects.get(RIG_OBJECT_NAME)
    sockets = [o for o in bpy.data.objects if o.name.startswith("Socket_")]

    exported = []
    for lod_name, col in lod_map.items():
        targets = []
        if col is not None:
            targets = [o for o in col.objects if o.type == "MESH" and not _is_technical(o)]
        if not targets and lod_name == "LOD0":
            # Fallback: all hero meshes for LOD0 only
            targets = list(hero_meshes)

        if not targets:
            report.add(
                f"Export Vortex_{lod_name}.fbx",
                False,
                f"no meshes in collection VORTEX_{lod_name}",
                level="warn",
            )
            continue

        # Select export set
        bpy.ops.object.select_all(action="DESELECT")
        for o in targets:
            o.select_set(True)
        if armature is not None and len(armature.data.bones) > 1:
            armature.select_set(True)
        # Skip provisional 1-bone placeholder armature on export
        for s in sockets:
            s.select_set(True)
        bpy.context.view_layer.objects.active = targets[0]

        out = EXPORT_DIR / f"Vortex_{lod_name}.fbx"
        bpy.ops.export_scene.fbx(
            filepath=str(out),
            use_selection=True,
            apply_scale_options="FBX_SCALE_ALL",
            axis_forward="-Z",
            axis_up="Y",
            add_leaf_bones=False,
            bake_anim=False,
            path_mode="COPY",
            embed_textures=True,
        )
        tris = sum(_count_tris(o) for o in targets)
        report.add(
            f"Exported `{out.name}`",
            out.is_file(),
            f"meshes={len(targets)} tris≈{tris} path={out}",
        )
        exported.append(out)

    if not exported:
        report.notes.append("Nenhum FBX gerado. Associe malhas reais às coleções VORTEX_LOD*.")

    return report


if __name__ == "__main__":
    from vortex_common import REPORTS_DIR

    r = export_fbx()
    out = r.write(REPORTS_DIR / "06_export_fbx.txt")
    print(r.to_text())
    print(f"Wrote {out}")
