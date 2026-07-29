"""Diagnose Castle_Tier1.glb only: hierarchy, mesh, UV, materials."""
from __future__ import annotations

import json
from pathlib import Path

import bpy
from mathutils import Vector

SRC = Path(r"C:\Valgor_Studio\production\City\Castle\source\Castle_Tier1.glb")
OUT = Path(r"C:\Valgor_Studio\docs\releases\beta-0.2.4-tier1-evidence\tier1_glb_diagnosis.json")
PREVIEW = Path(r"C:\Valgor_Studio\docs\releases\beta-0.2.4-tier1-evidence\tripo-tier1-blender.png")


def bounds(meshes):
    mn = Vector((1e9, 1e9, 1e9))
    mx = Vector((-1e9, -1e9, -1e9))
    for o in meshes:
        for c in o.bound_box:
            w = o.matrix_world @ Vector(c)
            mn = Vector((min(mn.x, w.x), min(mn.y, w.y), min(mn.z, w.z)))
            mx = Vector((max(mx.x, w.x), max(mx.y, w.y), max(mx.z, w.z)))
    return mn, mx


def main():
    OUT.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(SRC))

    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    report = {
        "source": str(SRC),
        "bytes": SRC.stat().st_size,
        "objects": [],
        "meshes": [],
        "materials": [],
        "images": [],
    }
    for o in bpy.context.scene.objects:
        report["objects"].append(
            {
                "name": o.name,
                "type": o.type,
                "parent": o.parent.name if o.parent else None,
                "loc": list(o.location),
                "scale": list(o.scale),
                "rot_deg": [round(a * 57.2957795, 3) for a in o.rotation_euler],
            }
        )

    for o in meshes:
        me = o.data
        uv0 = me.uv_layers[0] if me.uv_layers else None
        samples = []
        nonzero = 0
        if uv0:
            for i, loop in enumerate(uv0.data):
                if abs(loop.uv.x) > 1e-8 or abs(loop.uv.y) > 1e-8:
                    nonzero += 1
                if i < 6:
                    samples.append([round(loop.uv.x, 5), round(loop.uv.y, 5)])
        mat_indices = sorted({p.material_index for p in me.polygons})
        report["meshes"].append(
            {
                "name": o.name,
                "verts": len(me.vertices),
                "polys": len(me.polygons),
                "uv_layers": [u.name for u in me.uv_layers],
                "uv0_ok": nonzero > 0,
                "uv0_nonzero": nonzero,
                "uv0_sample": samples,
                "material_slots": [s.material.name if s.material else None for s in o.material_slots],
                "submesh_material_indices": mat_indices,
                "has_custom_normals": me.has_custom_normals,
            }
        )

    for mat in bpy.data.materials:
        entry = {"name": mat.name, "images": [], "roughness": None, "metallic": None}
        if mat.use_nodes and mat.node_tree:
            for n in mat.node_tree.nodes:
                if n.type == "TEX_IMAGE" and n.image:
                    entry["images"].append(n.image.name)
                if n.type == "BSDF_PRINCIPLED":
                    if n.inputs.get("Roughness"):
                        entry["roughness"] = float(n.inputs["Roughness"].default_value)
                    if n.inputs.get("Metallic"):
                        entry["metallic"] = float(n.inputs["Metallic"].default_value)
                    bc = n.inputs.get("Base Color")
                    if bc and bc.is_linked:
                        fn = bc.links[0].from_node
                        entry["base_color_from"] = fn.image.name if fn.type == "TEX_IMAGE" and fn.image else fn.type
        report["materials"].append(entry)

    for img in bpy.data.images:
        report["images"].append(
            {
                "name": img.name,
                "size": list(img.size) if img.size else None,
                "packed": bool(img.packed_file),
                "has_data": bool(img.has_data),
            }
        )

    if meshes:
        mn, mx = bounds(meshes)
        size = mx - mn
        report["bounds"] = {
            "min": [mn.x, mn.y, mn.z],
            "max": [mx.x, mx.y, mx.z],
            "size": [size.x, size.y, size.z],
            "footprint": float(max(size.x, size.y)),
            "height": float(size.z),
        }

    # Blender preview render
    center = (Vector(report["bounds"]["min"]) + Vector(report["bounds"]["max"])) * 0.5
    radius = max(report["bounds"]["size"])
    cam_data = bpy.data.cameras.new("Cam")
    cam = bpy.data.objects.new("Cam", cam_data)
    bpy.context.scene.collection.objects.link(cam)
    bpy.context.scene.camera = cam
    dist = radius * 2.1
    cam.location = (center.x + dist * 0.7, center.y - dist * 0.7, center.z + dist * 0.55)
    cam.rotation_euler = (center - cam.location).to_track_quat("-Z", "Y").to_euler()
    light_data = bpy.data.lights.new("Sun", type="SUN")
    light_data.energy = 2.5
    light = bpy.data.objects.new("Sun", light_data)
    bpy.context.scene.collection.objects.link(light)
    light.rotation_euler = (0.85, 0.2, 0.35)
    scene = bpy.context.scene
    engines = [i.identifier for i in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items]
    scene.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in engines else "BLENDER_EEVEE"
    scene.render.resolution_x = 1280
    scene.render.resolution_y = 720
    scene.render.filepath = str(PREVIEW)
    scene.render.image_settings.file_format = "PNG"
    bpy.ops.render.render(write_still=True)

    OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(
        "T1_DIAG",
        json.dumps(
            {
                "meshes": len(report["meshes"]),
                "mats": len(report["materials"]),
                "imgs": len(report["images"]),
                "uv0": report["meshes"][0]["uv0_ok"] if report["meshes"] else False,
                "fp": report.get("bounds", {}).get("footprint"),
                "preview": str(PREVIEW),
            }
        ),
    )


if __name__ == "__main__":
    main()
