"""
VALGOR — Fix Vortex heroic body proportions in Blender.

Corrects short/squashed ("dwarf") silhouette toward:
- height ~2.05 m
- ~8.25 head units
- longer legs, slightly smaller head relative to body
- pivot at feet, transforms applied

Does NOT rebuild identity/art from scratch.
- If a real imported mesh/armature exists: proportion-correct via armature rest bones + height fit.
- Always refreshes the technical heroic proportion GUIDE (not final art).
- Repositions sockets to heroic landmarks.
- Renders front / 3-4 / side / back previews.
- Does NOT enable Unity export.

Usage:
  blender -b Vortex_Production.blend --python fix_vortex_proportions.py
"""

from __future__ import annotations

import math
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import bpy
from mathutils import Vector, Matrix

from vortex_common import (
    BLEND_PATH,
    HEIGHT_REF_NAME,
    IMPORT_TAG_BASE,
    PROP_APPROVED_ART,
    PROP_IMPORT_TAG,
    PROP_UNITY_EXPORT_ALLOWED,
    PROP_VISUAL_APPROVED,
    REPORTS_DIR,
    RIG_OBJECT_NAME,
    TARGET_HEIGHT_M,
    TECHNICAL_OBJECT_NAMES,
)
from vortex_report import Report

HEAD_UNITS = 8.25
GUIDE_NAME = "Vortex_HeroicProportionGuide"
PREVIEWS_DIR = ROOT / "previews" / "proportion_fix"


def head_unit(h: float = TARGET_HEIGHT_M) -> float:
    return h / HEAD_UNITS


def landmarks(h: float = TARGET_HEIGHT_M) -> dict[str, float]:
    u = head_unit(h)
    return {
        "feet": 0.0,
        "ankle": 0.05 * u,
        "knee": 2.05 * u,
        "crotch": 4.05 * u,
        "navel": 5.05 * u,
        "chest": 6.05 * u,
        "shoulder": 6.55 * u,
        "chin": 7.45 * u,
        "head_top": h,
        "head": u,
    }


def _world_mesh_bounds(objects):
    coords = []
    for obj in objects:
        if obj.type != "MESH" or not obj.data or len(obj.data.vertices) == 0:
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


def _ensure_collection(name: str):
    col = bpy.data.collections.get(name)
    if col is None:
        col = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(col)
    return col


def _unlink_all(obj):
    for c in list(obj.users_collection):
        c.objects.unlink(obj)


def hero_meshes():
    out = []
    for o in bpy.data.objects:
        if o.type != "MESH":
            continue
        if o.name in TECHNICAL_OBJECT_NAMES or o.name == GUIDE_NAME or o.name.startswith(GUIDE_NAME):
            continue
        if o.name == HEIGHT_REF_NAME or o.name == "GroundReference":
            continue
        out.append(o)
    return out


def hero_armatures():
    out = []
    for o in bpy.data.objects:
        if o.type != "ARMATURE":
            continue
        if o.name == RIG_OBJECT_NAME and o.get(PROP_IMPORT_TAG) != IMPORT_TAG_BASE:
            # Keep placeholder unless it has a rich bone set
            if len(o.data.bones) <= 1:
                continue
        out.append(o)
    return out


def fit_height_feet(objects, report: Report):
    meshes = [o for o in objects if o.type == "MESH"]
    bounds = _world_mesh_bounds(meshes)
    if bounds is None:
        report.add("Height fit", False, "no hero mesh", level="warn")
        return
    min_c, max_c = bounds
    height = max_c.z - min_c.z
    top = [o for o in objects if o.parent is None or o.parent not in objects]
    if height < 1e-4:
        report.add("Height fit", False, "degenerate height")
        return
    factor = TARGET_HEIGHT_M / height
    for o in top:
        o.scale *= factor
    bpy.context.view_layer.update()
    bounds2 = _world_mesh_bounds(meshes)
    if bounds2:
        min2, max2 = bounds2
        for o in top:
            o.location.x += -((min2.x + max2.x) * 0.5)
            o.location.y += -((min2.y + max2.y) * 0.5)
            o.location.z += -min2.z
        bpy.context.view_layer.update()
    report.add("Global scale to 2.05 m", True, f"pre_h={height:.3f} factor={factor:.4f}")


def apply_transforms(objects, report: Report):
    targets = [o for o in objects if o.name in bpy.data.objects]
    if not targets:
        return
    _deselect_all()
    for o in targets:
        o.select_set(True)
    bpy.context.view_layer.objects.active = targets[0]
    try:
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        report.add("Apply transforms (Loc/Rot/Scale)", True, f"n={len(targets)}")
    except Exception as ex:  # noqa: BLE001
        report.add("Apply transforms", False, str(ex), level="warn")


def _find_bone(arm, names: list[str]):
    for n in names:
        if n in arm.data.bones:
            return n
        for b in arm.data.bones:
            if b.name.lower() == n.lower():
                return b.name
    return None


def proportion_correct_armature(arm_obj, report: Report):
    """
    Stretch legs / slightly reduce head-neck stack in rest pose edit mode
    to push silhouette toward ~8.25 heads without rebuilding meshes.
    """
    bpy.context.view_layer.objects.active = arm_obj
    arm_obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    eb = arm_obj.data.edit_bones

    def bone(names):
        n = _find_bone(arm_obj, names)
        return eb.get(n) if n else None

    hips = bone(["Hips", "hip", "pelvis", "Root"])
    spine = bone(["Spine", "spine"])
    chest = bone(["Chest", "spine.001", "Spine1"])
    upper_chest = bone(["UpperChest", "spine.002", "Spine2"])
    neck = bone(["Neck", "neck"])
    head = bone(["Head", "head"])
    left_up_leg = bone(["LeftUpperLeg", "thigh.L", "LeftUpLeg", "mixamorig:LeftUpLeg"])
    right_up_leg = bone(["RightUpperLeg", "thigh.R", "RightUpLeg", "mixamorig:RightUpLeg"])
    left_leg = bone(["LeftLowerLeg", "shin.L", "LeftLeg", "mixamorig:LeftLeg"])
    right_leg = bone(["RightLowerLeg", "shin.R", "RightLeg", "mixamorig:RightLeg"])
    left_foot = bone(["LeftFoot", "foot.L", "mixamorig:LeftFoot"])
    right_foot = bone(["RightFoot", "foot.R", "mixamorig:RightFoot"])

    # Lengthen upper+lower legs ~12%, shrink head bone ~8%
    leg_pairs = [
        (left_up_leg, left_leg, left_foot),
        (right_up_leg, right_leg, right_foot),
    ]
    stretched = 0
    for up, low, foot in leg_pairs:
        for b, factor in ((up, 1.12), (low, 1.10)):
            if b is None:
                continue
            direction = (b.tail - b.head).normalized()
            length = (b.tail - b.head).length
            b.tail = b.head + direction * (length * factor)
            stretched += 1
        if foot is not None and low is not None:
            # Keep foot attached to new shin tip
            foot.head = low.tail.copy()
            foot_dir = (foot.tail - foot.head)
            if foot_dir.length > 1e-6:
                foot.tail = foot.head + foot_dir.normalized() * foot_dir.length

    if head is not None:
        direction = (head.tail - head.head).normalized()
        length = (head.tail - head.head).length
        head.tail = head.head + direction * (length * 0.92)

    # Slightly decompress torso: nudge chest upward if present
    for b, factor in ((chest, 1.04), (upper_chest, 1.03), (spine, 1.03)):
        if b is None:
            continue
        direction = (b.tail - b.head).normalized()
        length = (b.tail - b.head).length
        b.tail = b.head + direction * (length * factor)
        stretched += 1

    bpy.ops.object.mode_set(mode="OBJECT")
    report.add(
        "Armature proportion edit (legs+/head-)",
        stretched > 0,
        f"bone_edits={stretched} hips={hips is not None} head={head is not None}",
        level="warn" if stretched == 0 else "info",
    )


def rebuild_heroic_guide(report: Report):
    """Technical proportion mannequin (NOT final Vortex art)."""
    # Remove previous guide parts
    for o in list(bpy.data.objects):
        if o.name == GUIDE_NAME or o.name.startswith(GUIDE_NAME + "_"):
            bpy.data.objects.remove(o, do_unlink=True)

    col = _ensure_collection("VORTEX_MODEL")
    lm = landmarks()
    u = lm["head"]

    def add_mesh(name, primitive, loc, scale):
        if primitive == "uv_sphere":
            bpy.ops.mesh.primitive_uv_sphere_add(radius=1.0, location=loc)
        elif primitive == "cylinder":
            bpy.ops.mesh.primitive_cylinder_add(radius=1.0, depth=2.0, location=loc)
        else:
            bpy.ops.mesh.primitive_cube_add(size=2.0, location=loc)
        obj = bpy.context.object
        obj.name = f"{GUIDE_NAME}_{name}"
        obj.scale = scale
        obj[PROP_IMPORT_TAG] = "proportion_guide"
        obj[PROP_APPROVED_ART] = False
        _unlink_all(obj)
        col.objects.link(obj)
        # Dim guide material
        mat = bpy.data.materials.get("MAT_Vortex_ProportionGuide")
        if mat is None:
            mat = bpy.data.materials.new("MAT_Vortex_ProportionGuide")
            mat.use_fake_user = True
            mat.use_nodes = True
            bsdf = mat.node_tree.nodes.get("Principled BSDF")
            if bsdf:
                bsdf.inputs["Base Color"].default_value = (0.75, 0.55, 0.15, 1.0)
                if "Alpha" in bsdf.inputs:
                    bsdf.inputs["Alpha"].default_value = 0.55
            mat.blend_method = "BLEND"
        if obj.data.materials:
            obj.data.materials[0] = mat
        else:
            obj.data.materials.append(mat)
        return obj

    # Head
    add_mesh("Head", "uv_sphere", (0, 0, lm["chin"] + u * 0.5), (u * 0.45, u * 0.45, u * 0.5))
    # Torso
    torso_h = lm["chin"] - lm["crotch"]
    add_mesh("Torso", "cylinder", (0, 0, lm["crotch"] + torso_h * 0.5), (0.17, 0.12, torso_h * 0.5))
    # Hips
    add_mesh("Hips", "cube", (0, 0, lm["crotch"]), (0.18, 0.12, 0.07))
    # Legs
    thigh_h = lm["crotch"] - lm["knee"]
    shin_h = lm["knee"] - 0.02
    for side, x in (("L", -0.09), ("R", 0.09)):
        add_mesh(f"Thigh_{side}", "cylinder", (x, 0, lm["knee"] + thigh_h * 0.5), (0.06, 0.06, thigh_h * 0.5))
        add_mesh(f"Shin_{side}", "cylinder", (x, 0, 0.02 + shin_h * 0.5), (0.05, 0.05, shin_h * 0.5))
    # Arms
    arm_h = 3.1 * u
    for side, x in (("L", -0.28), ("R", 0.28)):
        add_mesh(f"Arm_{side}", "cylinder", (x, 0, lm["shoulder"] - arm_h * 0.35), (0.045, 0.045, arm_h * 0.5))

    # Apply transforms on guide
    guide_objs = [o for o in bpy.data.objects if o.name.startswith(GUIDE_NAME)]
    apply_transforms(guide_objs, report)
    report.add("Heroic proportion guide rebuilt", True, f"parts={len(guide_objs)} head_units={HEAD_UNITS}")


def reposition_sockets(report: Report):
    lm = landmarks()
    mapping = {
        "Socket_RightHand": (0.48, 0.05, lm["shoulder"] - 0.35),
        "Socket_LeftHand": (-0.48, 0.05, lm["shoulder"] - 0.35),
        "Socket_BackWeapon": (0.0, -0.20, lm["chest"]),
        "Socket_HipWeapon": (0.28, 0.05, lm["crotch"] + 0.05),
        "Socket_HeadVFX": (0.0, 0.0, lm["head_top"]),
        "Socket_ChestVFX": (0.0, 0.12, lm["chest"]),
        "Socket_FootLeftVFX": (-0.12, 0.05, 0.04),
        "Socket_FootRightVFX": (0.12, 0.05, 0.04),
        "Socket_DragonLink": (0.0, -0.28, lm["shoulder"]),
    }
    n = 0
    for name, loc in mapping.items():
        obj = bpy.data.objects.get(name)
        if obj is None:
            continue
        obj.location = Vector(loc)
        n += 1
    report.add("Sockets repositioned to heroic landmarks", n >= 9, f"updated={n}")


def update_height_reference(report: Report):
    ref = bpy.data.objects.get(HEIGHT_REF_NAME)
    if ref is None:
        bpy.ops.mesh.primitive_cube_add(location=(0.6, 0, TARGET_HEIGHT_M * 0.5), scale=(0.02, 0.02, TARGET_HEIGHT_M * 0.5))
        ref = bpy.context.object
        ref.name = HEIGHT_REF_NAME
    else:
        ref.location = (0.6, 0.0, TARGET_HEIGHT_M * 0.5)
        ref.scale = (0.02, 0.02, TARGET_HEIGHT_M * 0.5)
    _deselect_all()
    ref.select_set(True)
    bpy.context.view_layer.objects.active = ref
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    ref.location = (0.6, 0.0, TARGET_HEIGHT_M * 0.5)
    report.add("Height reference 2.05 m refreshed", True)


def setup_studio_camera():
    cam_data = bpy.data.cameras.get("VortexPreviewCamData") or bpy.data.cameras.new("VortexPreviewCamData")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = 2.4
    cam = bpy.data.objects.get("VortexPreviewCamera")
    if cam is None:
        cam = bpy.data.objects.new("VortexPreviewCamera", cam_data)
        bpy.context.scene.collection.objects.link(cam)
    else:
        cam.data = cam_data
    bpy.context.scene.camera = cam

    # Soft light
    light = bpy.data.objects.get("VortexPreviewLight")
    if light is None:
        light_data = bpy.data.lights.new("VortexPreviewLightData", type="AREA")
        light_data.energy = 120
        light_data.size = 3
        light = bpy.data.objects.new("VortexPreviewLight", light_data)
        bpy.context.scene.collection.objects.link(light)
    light.location = (2.0, -2.0, 2.5)

    world = bpy.context.scene.world
    if world is None:
        world = bpy.data.worlds.new("VortexPreviewWorld")
        bpy.context.scene.world = world
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs[0].default_value = (0.06, 0.07, 0.09, 1.0)
        bg.inputs[1].default_value = 1.0
    return cam


def render_previews(report: Report):
    PREVIEWS_DIR.mkdir(parents=True, exist_ok=True)
    cam = setup_studio_camera()
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE" if "BLENDER_EEVEE" in dir(bpy.types) else scene.render.engine
    # Blender 5 may use EEVEE_NEXT
    if hasattr(bpy.types, "View3DEEVEE"):
        pass
    try:
        scene.render.engine = "BLENDER_EEVEE_NEXT"
    except Exception:
        try:
            scene.render.engine = "BLENDER_EEVEE"
        except Exception:
            scene.render.engine = "CYCLES"

    scene.render.resolution_x = 768
    scene.render.resolution_y = 1152
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False

    views = {
        "front": ((0.0, -3.2, 1.02), (math.radians(90), 0, 0)),
        "three_quarter": ((2.2, -2.4, 1.05), (math.radians(78), 0, math.radians(40))),
        "side": ((3.2, 0.0, 1.02), (math.radians(90), 0, math.radians(90))),
        "back": ((0.0, 3.2, 1.02), (math.radians(90), 0, math.radians(180))),
    }

    # Ortho look-at via matrix
    for name, (loc, _) in views.items():
        cam.location = Vector(loc)
        # Aim at character center
        direction = Vector((0.0, 0.0, 1.02)) - cam.location
        cam.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
        if cam.data.type == "ORTHO":
            cam.data.ortho_scale = 2.35
        out = PREVIEWS_DIR / f"Vortex_proportion_{name}.png"
        scene.render.filepath = str(out)
        bpy.ops.render.render(write_still=True)
        report.add(f"Preview `{name}`", out.is_file(), str(out))


def measure_report(objects, report: Report):
    meshes = [o for o in objects if o.type == "MESH"]
    bounds = _world_mesh_bounds(meshes)
    if not bounds:
        report.add("Measured height", False, "no mesh", level="warn")
        return
    min_c, max_c = bounds
    h = max_c.z - min_c.z
    hu = h / head_unit(h) if h > 0 else 0
    # Approximate head units using measured height / assumed head from top 12%
    report.add(
        "Standing height ~2.05 m",
        abs(h - TARGET_HEIGHT_M) <= 0.06,
        f"h={h:.3f} min_z={min_c.z:.3f}",
    )
    report.add(
        "Feet near Z=0",
        abs(min_c.z) <= 0.05,
        f"min_z={min_c.z:.3f}",
    )
    report.notes.append(
        f"Target head-units={HEAD_UNITS}. Guide/mannnequin uses heroic landmarks; "
        "imported mesh correction uses armature rest-pose edits when bones exist."
    )


def main() -> int:
    report = Report("Vortex — Proportion Fix")
    REPORTS_DIR.mkdir(parents=True, exist_ok=True)

    # Never unlock Unity from this tool
    scene = bpy.context.scene
    if PROP_VISUAL_APPROVED not in scene.keys():
        scene[PROP_VISUAL_APPROVED] = False
    if PROP_UNITY_EXPORT_ALLOWED not in scene.keys():
        scene[PROP_UNITY_EXPORT_ALLOWED] = False
    # Keep gates closed after proportion fix
    scene[PROP_VISUAL_APPROVED] = False
    scene[PROP_UNITY_EXPORT_ALLOWED] = False
    scene["VALGOR_HEIGHT_M"] = TARGET_HEIGHT_M
    scene["VALGOR_HEAD_UNITS"] = HEAD_UNITS

    meshes = hero_meshes()
    arms = hero_armatures()
    report.add("Hero mesh present", len(meshes) > 0, f"count={len(meshes)}", level="warn")
    report.add("Hero armature present", len(arms) > 0, f"count={len(arms)}", level="warn")

    targets = list({*meshes, *arms})
    if targets:
        # Do not touch approved art geometry identity — only proportion/transform
        fit_height_feet(targets, report)
        for arm in arms:
            proportion_correct_armature(arm, report)
        fit_height_feet(targets, report)
        apply_transforms(targets, report)
        measure_report(meshes, report)
    else:
        report.notes.append(
            "Nenhuma malha artística importada no .blend. "
            "Atualizando guia de proporção heroica + sockets + previews. "
            "Coloque Vortex_Base.glb/fbx em source/ e rode import + este script."
        )

    update_height_reference(report)
    rebuild_heroic_guide(report)
    reposition_sockets(report)

    # Measure guide as presentation stand-in
    guide = [o for o in bpy.data.objects if o.name.startswith(GUIDE_NAME)]
    measure_report(guide, report)
    render_previews(report)

    report.notes.extend(
        [
            "Identidade visual (armadura/capa/barba/cabelo) preservada quando malha real existir.",
            "Guia Vortex_HeroicProportionGuide_* é técnico — NÃO é arte final.",
            "Unity export permanece bloqueado até mark_visual_approved.py.",
            f"Previews: {PREVIEWS_DIR}",
        ]
    )

    out = report.write(REPORTS_DIR / "12_proportion_fix.txt")
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    print(report.to_text())
    print(f"Wrote {out}")
    print(f"Saved {BLEND_PATH}")
    return 0 if report.failed == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
