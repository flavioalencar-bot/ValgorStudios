"""
Post-process Tripo Vortex_Base after import:
- rename mesh
- heroic silhouette warp (longer legs / slightly smaller head) without armature
- feet pivot, height 2.05 m, transforms applied
- face toward Blender -Y (Unity +Z with standard FBX export)
- move technical guide to hidden collection
- re-render clean previews of the real mesh only
- keep Unity export gated off
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
    IMPORT_TAG_BASE,
    PROP_IMPORT_TAG,
    PROP_UNITY_EXPORT_ALLOWED,
    PROP_VISUAL_APPROVED,
    REPORTS_DIR,
    TARGET_HEIGHT_M,
)
from vortex_report import Report

GUIDE_PREFIX = "Vortex_HeroicProportionGuide"
PREVIEWS_DIR = ROOT / "previews" / "proportion_fix"
HEAD_UNITS = 8.25


def find_base_mesh():
    candidates = []
    for o in bpy.data.objects:
        if o.type != "MESH":
            continue
        if o.name.startswith(GUIDE_PREFIX):
            continue
        if o.name in {"GroundReference", "Vortex_HeightReference_2_05m"}:
            continue
        if o.get(PROP_IMPORT_TAG) == IMPORT_TAG_BASE or "tripo" in o.name.lower() or o.name.startswith("Vortex_Base"):
            candidates.append(o)
    if candidates:
        return candidates[0]
    # fallback: largest non-technical mesh
    best = None
    best_n = 0
    for o in bpy.data.objects:
        if o.type != "MESH" or o.name.startswith(GUIDE_PREFIX):
            continue
        if o.name in {"GroundReference", "Vortex_HeightReference_2_05m"}:
            continue
        n = len(o.data.vertices)
        if n > best_n:
            best = o
            best_n = n
    return best


def bounds_world(obj):
    coords = [obj.matrix_world @ v.co for v in obj.data.vertices]
    xs = [c.x for c in coords]
    ys = [c.y for c in coords]
    zs = [c.z for c in coords]
    return Vector((min(xs), min(ys), min(zs))), Vector((max(xs), max(ys), max(zs)))


def apply_obj_transforms(obj):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


def fit_height_feet(obj, report: Report):
    bpy.context.view_layer.update()
    mn, mx = bounds_world(obj)
    h = mx.z - mn.z
    if h < 1e-6:
        report.add("Height fit", False, "degenerate")
        return
    factor = TARGET_HEIGHT_M / h
    obj.scale *= factor
    bpy.context.view_layer.update()
    mn, mx = bounds_world(obj)
    obj.location.x += -((mn.x + mx.x) * 0.5)
    obj.location.y += -((mn.y + mx.y) * 0.5)
    obj.location.z += -mn.z
    bpy.context.view_layer.update()
    apply_obj_transforms(obj)
    mn2, mx2 = bounds_world(obj)
    report.add(
        "Height/pivot after fit",
        abs((mx2.z - mn2.z) - TARGET_HEIGHT_M) < 0.02 and abs(mn2.z) < 0.02,
        f"h={mx2.z - mn2.z:.4f} min_z={mn2.z:.4f}",
    )


def heroic_vertex_warp(obj, report: Report):
    """
    Non-uniform Z remapping to lengthen lower body and slightly compress head band.
    Keeps feet at 0 and total height at TARGET_HEIGHT_M.
    """
    mesh = obj.data
    zs = [v.co.z for v in mesh.vertices]
    z0, z1 = min(zs), max(zs)
    h = z1 - z0
    if h < 1e-6:
        report.add("Vertex warp", False, "no height")
        return

    # Normalized landmarks in [0,1]
    # stretch 0..0.50 (legs) , keep mid, compress 0.88..1.0 (head)
    def remap(t: float) -> float:
        t = max(0.0, min(1.0, t))
        # piecewise linear map designed for ~8.25 heads feel on short-legged scans
        keys = [
            (0.00, 0.00),
            (0.18, 0.28),  # lower leg stretch
            (0.40, 0.52),  # crotch pushed toward mid-height
            (0.62, 0.68),
            (0.82, 0.86),
            (0.92, 0.93),  # head band compress start
            (1.00, 1.00),
        ]
        for i in range(len(keys) - 1):
            a0, b0 = keys[i]
            a1, b1 = keys[i + 1]
            if t <= a1 or i == len(keys) - 2:
                u = 0 if a1 == a0 else (t - a0) / (a1 - a0)
                return b0 + (b1 - b0) * u
        return t

    for v in mesh.vertices:
        t = (v.co.z - z0) / h
        v.co.z = z0 + remap(t) * h
    mesh.update()
    report.add("Heroic Z silhouette warp applied", True, "legs+/head-band soft compress")
    fit_height_feet(obj, report)


def orient_face_neg_y(obj, report: Report):
    """
    Blender Z-up: character should face -Y so FBX (-Z Forward, Y Up) maps to Unity +Z.
    Heuristic: choose rotation 0/90/180/270 around Z that maximizes +depth of upper torso
    facing camera at -Y (prefer thinner depth on Y after orientation for humanoids).
    """
    best = None
    for deg in (0, 90, 180, 270):
        obj.rotation_euler[2] = math.radians(deg)
        bpy.context.view_layer.update()
        mn, mx = bounds_world(obj)
        sx = mx.x - mn.x
        sy = mx.y - mn.y
        # Prefer facing where depth (Y) is smaller than width (X) for a character standing in T/A pose-ish
        score = sx - sy  # higher => wider than deep => likely front/back aligned to Y
        if best is None or score > best[0]:
            best = (score, deg, sx, sy)
    obj.rotation_euler[2] = math.radians(best[1])
    bpy.context.view_layer.update()
    apply_obj_transforms(obj)
    fit_height_feet(obj, report)
    report.add(
        "Oriented for Unity +Z forward (Blender face -Y heuristic)",
        True,
        f"rot_z={best[1]} width_x={best[2]:.3f} depth_y={best[3]:.3f}",
    )


def hide_guides(report: Report):
    col = bpy.data.collections.get("VORTEX_GUIDE")
    if col is None:
        col = bpy.data.collections.new("VORTEX_GUIDE")
        bpy.context.scene.collection.children.link(col)
    n = 0
    for o in list(bpy.data.objects):
        if o.name.startswith(GUIDE_PREFIX) or o.name == "Vortex_HeightReference_2_05m":
            for c in list(o.users_collection):
                c.objects.unlink(o)
            if o.name not in col.objects:
                col.objects.link(o)
            o.hide_render = True
            o.hide_viewport = True
            n += 1
    col.hide_render = True
    col.hide_viewport = True
    report.add("Technical guides hidden from preview", True, f"moved={n}")


def organize_single_mesh(obj, report: Report):
    mapping = {
        "VORTEX_MODEL": True,
        "VORTEX_LOD0": True,
        "VORTEX_ARMOR": True,  # Tripo bake includes armor in same mesh
        "VORTEX_CAPE": True,
        "VORTEX_WEAPON": False,  # unknown if separate — keep false unless name hints
    }
    # Always ensure collections exist
    for name in ("VORTEX_MODEL", "VORTEX_LOD0", "VORTEX_ARMOR", "VORTEX_CAPE", "VORTEX_WEAPON", "VORTEX_RIG"):
        if name not in bpy.data.collections:
            c = bpy.data.collections.new(name)
            bpy.context.scene.collection.children.link(c)

    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    for name, enable in mapping.items():
        if enable:
            bpy.data.collections[name].objects.link(obj)

    obj.name = "Vortex_Base"
    if obj.data:
        obj.data.name = "Vortex_Base_Mesh"

    # Materials: keep Tripo mat, ensure placeholders exist with fake user
    from ensure_placeholder_materials import ensure_placeholder_materials

    ensure_placeholder_materials()
    report.add(
        "Organized Vortex_Base (single Tripo mesh)",
        True,
        "linked MODEL/LOD0/ARMOR/CAPE — body/hair/beard not separable yet (one mesh)",
    )


def render_previews(report: Report):
    PREVIEWS_DIR.mkdir(parents=True, exist_ok=True)
    cam = bpy.data.objects.get("VortexPreviewCamera")
    if cam is None:
        cam_data = bpy.data.cameras.new("VortexPreviewCamData")
        cam = bpy.data.objects.new("VortexPreviewCamera", cam_data)
        bpy.context.scene.collection.objects.link(cam)
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = 2.35
    bpy.context.scene.camera = cam

    scene = bpy.context.scene
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

    views = {
        "front": Vector((0.0, -3.2, 1.02)),
        "three_quarter": Vector((2.2, -2.4, 1.05)),
        "side": Vector((3.2, 0.0, 1.02)),
        "back": Vector((0.0, 3.2, 1.02)),
    }
    target = Vector((0.0, 0.0, 1.02))
    for name, loc in views.items():
        cam.location = loc
        cam.rotation_euler = (target - loc).to_track_quat("-Z", "Y").to_euler()
        out = PREVIEWS_DIR / f"Vortex_proportion_{name}.png"
        scene.render.filepath = str(out)
        bpy.ops.render.render(write_still=True)
        report.add(f"Preview `{name}`", out.is_file(), str(out))


def main() -> int:
    report = Report("Vortex — Tripo Post-Import Refine")
    REPORTS_DIR.mkdir(parents=True, exist_ok=True)

    scene = bpy.context.scene
    scene[PROP_VISUAL_APPROVED] = False
    scene[PROP_UNITY_EXPORT_ALLOWED] = False
    scene["VALGOR_HEIGHT_M"] = TARGET_HEIGHT_M
    scene["VALGOR_HEAD_UNITS"] = HEAD_UNITS

    obj = find_base_mesh()
    if obj is None:
        report.add("Vortex base mesh", False, "not found")
        report.write(REPORTS_DIR / "13_tripo_refine.txt")
        print(report.to_text())
        return 1

    report.add("Vortex base mesh found", True, obj.name)
    organize_single_mesh(obj, report)
    fit_height_feet(obj, report)
    heroic_vertex_warp(obj, report)
    orient_face_neg_y(obj, report)
    hide_guides(report)
    render_previews(report)

    mn, mx = bounds_world(obj)
    report.add(
        "Final height 2.05 m / feet pivot",
        abs((mx.z - mn.z) - TARGET_HEIGHT_M) < 0.03 and abs(mn.z) < 0.03,
        f"h={mx.z - mn.z:.4f} min_z={mn.z:.4f}",
    )
    report.notes.extend(
        [
            "Unity export BLOCKED (VALGOR_VISUAL_APPROVED=False).",
            "Tripo delivered a single fused mesh — split body/hair/beard/armor/cape/weapon requires manual separation or new export with parts.",
            "No Humanoid armature in GLB — rig still pending.",
            f"Previews: {PREVIEWS_DIR}",
        ]
    )

    out = report.write(REPORTS_DIR / "13_tripo_refine.txt")
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    print(report.to_text())
    print(f"Wrote {out}")
    return 0 if report.failed == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
