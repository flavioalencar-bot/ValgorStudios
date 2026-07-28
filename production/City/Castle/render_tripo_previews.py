"""Render orthographic preview of each source GLB for Tripo comparison."""
from __future__ import annotations

from pathlib import Path

import bpy
from mathutils import Vector

SRC = Path(r"C:\Valgor_Studio\production\City\Castle\source")
OUT = Path(r"C:\Valgor_Studio\docs\releases\beta-0.2.4-evidence\tripo-source-previews")
OUT.mkdir(parents=True, exist_ok=True)


def bounds(meshes):
    mn = Vector((1e9, 1e9, 1e9))
    mx = Vector((-1e9, -1e9, -1e9))
    for o in meshes:
        for c in o.bound_box:
            w = o.matrix_world @ Vector(c)
            mn = Vector((min(mn.x, w.x), min(mn.y, w.y), min(mn.z, w.z)))
            mx = Vector((max(mx.x, w.x), max(mx.y, w.y), max(mx.z, w.z)))
    return mn, mx


def render_tier(tier: int):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    path = SRC / f"Castle_Tier{tier}.glb"
    bpy.ops.import_scene.gltf(filepath=str(path))
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    mn, mx = bounds(meshes)
    center = (mn + mx) * 0.5
    size = mx - mn
    radius = max(size.x, size.y, size.z)

    # Camera
    cam_data = bpy.data.cameras.new("Cam")
    cam = bpy.data.objects.new("Cam", cam_data)
    bpy.context.scene.collection.objects.link(cam)
    bpy.context.scene.camera = cam
    dist = radius * 2.2
    cam.location = (center.x + dist * 0.7, center.y - dist * 0.7, center.z + dist * 0.55)
    direction = center - cam.location
    cam.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()

    # Light
    light_data = bpy.data.lights.new("L", type="SUN")
    light_data.energy = 2.5
    light = bpy.data.objects.new("L", light_data)
    bpy.context.scene.collection.objects.link(light)
    light.rotation_euler = (0.8, 0.2, 0.3)

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items.keys() else "BLENDER_EEVEE"
    scene.render.resolution_x = 1280
    scene.render.resolution_y = 720
    scene.render.filepath = str(OUT / f"tripo-tier-{tier}.png")
    scene.render.image_settings.file_format = "PNG"
    bpy.ops.render.render(write_still=True)
    print("PREVIEW", tier, scene.render.filepath)


def main():
    for t in range(1, 7):
        render_tier(t)
    print("DONE")


if __name__ == "__main__":
    main()
