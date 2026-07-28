"""Deep diagnose Castle Tier GLBs vs staged FBX: meshes, UVs, materials, transforms."""
from __future__ import annotations

import json
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(r"C:\Valgor_Studio\production\City\Castle")
SOURCE = ROOT / "source"
STAGING = ROOT / "unity_staging" / "Models"
OUT = ROOT / "reports" / "castle_import_diagnosis.json"


def clear():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def world_bounds(meshes):
    mn = Vector((1e9, 1e9, 1e9))
    mx = Vector((-1e9, -1e9, -1e9))
    for o in meshes:
        for corner in o.bound_box:
            w = o.matrix_world @ Vector(corner)
            mn = Vector((min(mn.x, w.x), min(mn.y, w.y), min(mn.z, w.z)))
            mx = Vector((max(mx.x, w.x), max(mx.y, w.y), max(mx.z, w.z)))
    return mn, mx


def analyze_scene(label: str) -> dict:
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    info = {
        "label": label,
        "object_count": len(bpy.context.scene.objects),
        "mesh_count": len(meshes),
        "meshes": [],
        "materials": [],
        "images": [],
        "hierarchy": [],
    }
    for o in bpy.context.scene.objects:
        info["hierarchy"].append(
            {
                "name": o.name,
                "type": o.type,
                "parent": o.parent.name if o.parent else None,
                "loc": list(o.location),
                "rot_euler_deg": [round(x * 57.2957795, 3) for x in o.rotation_euler],
                "scale": list(o.scale),
            }
        )

    for o in meshes:
        me = o.data
        uv_layers = [uv.name for uv in me.uv_layers]
        uv0_ok = False
        uv0_nonzero = 0
        uv0_sample = []
        if me.uv_layers:
            uv = me.uv_layers[0].data
            for i, loop in enumerate(uv):
                u, v = loop.uv.x, loop.uv.y
                if abs(u) > 1e-8 or abs(v) > 1e-8:
                    uv0_nonzero += 1
                if i < 8:
                    uv0_sample.append([round(u, 5), round(v, 5)])
            uv0_ok = uv0_nonzero > 0

        # material slots / polygon material indices
        mat_indices = sorted({p.material_index for p in me.polygons}) if me.polygons else []
        slot_names = [s.material.name if s.material else None for s in o.material_slots]

        has_normals = True  # mesh always has normals in Blender
        # custom split normals?
        has_custom_normals = me.has_custom_normals

        # tangents: ensure calc
        tangent_ok = False
        try:
            if me.uv_layers:
                me.calc_tangents()
                tangent_ok = True
        except Exception as ex:  # noqa: BLE001
            tangent_ok = False

        info["meshes"].append(
            {
                "name": o.name,
                "verts": len(me.vertices),
                "polys": len(me.polygons),
                "loops": len(me.loops),
                "submesh_material_indices": mat_indices,
                "material_slot_count": len(o.material_slots),
                "material_slots": slot_names,
                "uv_layers": uv_layers,
                "uv0_present": bool(me.uv_layers),
                "uv0_nonzero_loops": uv0_nonzero,
                "uv0_ok": uv0_ok,
                "uv0_sample": uv0_sample,
                "uv1_present": len(me.uv_layers) > 1,
                "has_custom_normals": has_custom_normals,
                "tangent_calc_ok": tangent_ok,
                "loc": list(o.location),
                "rot_euler_deg": [round(x * 57.2957795, 3) for x in o.rotation_euler],
                "scale": list(o.scale),
            }
        )

    for mat in bpy.data.materials:
        m = {"name": mat.name, "use_nodes": mat.use_nodes, "images": [], "principled": {}}
        if mat.use_nodes and mat.node_tree:
            for n in mat.node_tree.nodes:
                if n.type == "TEX_IMAGE" and n.image:
                    m["images"].append(n.image.name)
                if n.type == "BSDF_PRINCIPLED":
                    bc = n.inputs.get("Base Color")
                    linked = None
                    if bc and bc.is_linked:
                        fn = bc.links[0].from_node
                        linked = fn.image.name if fn.type == "TEX_IMAGE" and fn.image else fn.type
                    m["principled"]["base_color_linked"] = linked
                    if n.inputs.get("Roughness"):
                        m["principled"]["roughness"] = float(n.inputs["Roughness"].default_value)
                    if n.inputs.get("Metallic"):
                        m["principled"]["metallic"] = float(n.inputs["Metallic"].default_value)
        info["materials"].append(m)

    for img in bpy.data.images:
        info["images"].append(
            {
                "name": img.name,
                "size": list(img.size) if img.size else None,
                "packed": bool(img.packed_file),
                "has_data": bool(img.has_data),
            }
        )

    if meshes:
        mn, mx = world_bounds(meshes)
        size = mx - mn
        info["bounds"] = {
            "min": [mn.x, mn.y, mn.z],
            "max": [mx.x, mx.y, mx.z],
            "size": [size.x, size.y, size.z],
            "footprint": float(max(size.x, size.y)),
            "height": float(size.z),
        }
    return info


def load_glb(path: Path):
    clear()
    bpy.ops.import_scene.gltf(filepath=str(path))


def load_fbx(path: Path):
    clear()
    bpy.ops.import_scene.fbx(filepath=str(path))


def main():
    report = {"tiers": []}
    for tier in range(1, 7):
        glb = SOURCE / f"Castle_Tier{tier}.glb"
        fbx = STAGING / f"Castle_Tier{tier}.fbx"
        entry = {"tier": tier, "glb": None, "fbx_staging": None, "issues": []}
        if glb.is_file():
            load_glb(glb)
            entry["glb"] = analyze_scene(f"glb_t{tier}")
            entry["glb"]["bytes"] = glb.stat().st_size
        else:
            entry["issues"].append("missing_glb")
        if fbx.is_file():
            load_fbx(fbx)
            entry["fbx_staging"] = analyze_scene(f"fbx_t{tier}")
            entry["fbx_staging"]["bytes"] = fbx.stat().st_size
        else:
            entry["issues"].append("missing_fbx")

        # Compare UV integrity
        if entry["glb"] and entry["fbx_staging"]:
            g = entry["glb"]["meshes"][0] if entry["glb"]["meshes"] else {}
            f = entry["fbx_staging"]["meshes"][0] if entry["fbx_staging"]["meshes"] else {}
            if g.get("verts") != f.get("verts"):
                entry["issues"].append(f"vert_mismatch glb={g.get('verts')} fbx={f.get('verts')}")
            if g.get("uv0_ok") and not f.get("uv0_ok"):
                entry["issues"].append("fbx_lost_uv0")
            if g.get("polys") != f.get("polys"):
                entry["issues"].append(f"poly_mismatch glb={g.get('polys')} fbx={f.get('polys')}")
            # UV sample drift
            gs = g.get("uv0_sample") or []
            fs = f.get("uv0_sample") or []
            if gs and fs and gs != fs:
                entry["issues"].append("uv0_sample_differs_glb_vs_fbx")

        report["tiers"].append(entry)
        print("TIER", tier, "issues=", entry["issues"])

    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print("WROTE", OUT)


if __name__ == "__main__":
    main()
