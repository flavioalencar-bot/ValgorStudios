"""
Light polish after visual approval:
- slightly reduce boot/foot visual weight (XZ scale on lower vertices)
- improve rear rim lighting for back preview
- re-render back (+ optional front check)
Does NOT change identity/armor silhouette overall.
"""

from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import bpy
from mathutils import Vector

from vortex_common import BLEND_PATH, REPORTS_DIR, TARGET_HEIGHT_M
from vortex_report import Report

PREVIEWS_DIR = ROOT / "previews" / "proportion_fix"


def find_vortex_base():
    o = bpy.data.objects.get("Vortex_Base")
    if o and o.type == "MESH":
        return o
    for obj in bpy.data.objects:
        if obj.type == "MESH" and obj.get("VALGOR_IMPORT_TAG") == "vortex_base":
            return obj
    return None


def bounds(obj):
    coords = [obj.matrix_world @ v.co for v in obj.data.vertices]
    zs = [c.z for c in coords]
    return min(zs), max(zs)


def slim_boots(obj, report: Report):
    mesh = obj.data
    z0, z1 = bounds(obj)
    h = z1 - z0
    # Lower 9% of height: gently shrink XZ toward axis
    band = z0 + h * 0.09
    count = 0
    for v in mesh.vertices:
        world = obj.matrix_world @ v.co
        if world.z > band:
            continue
        t = 1.0 - (world.z - z0) / max(band - z0, 1e-6)  # 1 at feet, 0 at band
        shrink = 1.0 - 0.10 * max(0.0, min(1.0, t))  # up to -10% at soles
        # work in object space around origin (feet centered)
        v.co.x *= shrink
        v.co.y *= shrink
        count += 1
    mesh.update()
    report.add("Boot/foot visual weight reduced", count > 0, f"verts_touched={count}")


def ensure_rear_light(report: Report):
    light = bpy.data.objects.get("VortexRearRimLight")
    if light is None:
        data = bpy.data.lights.new("VortexRearRimLightData", type="AREA")
        data.energy = 90
        data.size = 2.5
        data.color = (0.85, 0.90, 1.0)
        light = bpy.data.objects.new("VortexRearRimLight", data)
        bpy.context.scene.collection.objects.link(light)
    light.location = (0.0, 2.8, 1.6)
    light.rotation_euler = (Vector((0, 0, 1.1)) - light.location).to_track_quat("-Z", "Y").to_euler()
    report.add("Rear rim light set", True, f"loc={tuple(round(c,2) for c in light.location)}")


def render_views(report: Report):
    PREVIEWS_DIR.mkdir(parents=True, exist_ok=True)
    cam = bpy.data.objects.get("VortexPreviewCamera")
    if cam is None:
        report.add("Preview camera", False, "missing")
        return
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
            pass
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
    report = Report("Vortex — Pre-Integration Polish")
    REPORTS_DIR.mkdir(parents=True, exist_ok=True)
    obj = find_vortex_base()
    if obj is None:
        report.add("Vortex_Base mesh", False, "not found")
        print(report.to_text())
        return 1
    slim_boots(obj, report)
    # Re-fit height after boot slim (should be unchanged in Z)
    bpy.context.view_layer.update()
    ensure_rear_light(report)
    render_views(report)
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    out = report.write(REPORTS_DIR / "14_pre_integration_polish.txt")
    print(report.to_text())
    print(f"Wrote {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
