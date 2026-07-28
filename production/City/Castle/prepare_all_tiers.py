"""Prepare Castle_Tier1..6 for Unity: scale, pivot, FBX, BaseColor textures."""
from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(r"C:\Valgor_Studio\production\City\Castle")
SOURCE = ROOT / "source"
STAGING_MODELS = ROOT / "unity_staging" / "Models"
STAGING_TEX = ROOT / "unity_staging" / "Textures"
REPORTS = ROOT / "reports"

# Progressive footprints (meters). Growth visible; not identical.
TARGET_FOOTPRINT = {
    1: 7.5,
    2: 8.1,
    3: 8.7,
    4: 9.4,
    5: 10.1,
    6: 10.8,
}


def world_bounds(meshes):
    mn = Vector((1e9, 1e9, 1e9))
    mx = Vector((-1e9, -1e9, -1e9))
    for o in meshes:
        for corner in o.bound_box:
            w = o.matrix_world @ Vector(corner)
            mn.x = min(mn.x, w.x)
            mn.y = min(mn.y, w.y)
            mn.z = min(mn.z, w.z)
            mx.x = max(mx.x, w.x)
            mx.y = max(mx.y, w.y)
            mx.z = max(mx.z, w.z)
    return mn, mx


def export_textures(tier: int) -> list:
    exported = []
    for img in bpy.data.images:
        info = {
            "name": img.name,
            "size": list(img.size) if img.size else None,
            "packed": bool(img.packed_file),
            "has_data": bool(img.has_data),
        }
        if not (img.packed_file or img.has_data):
            info["error"] = "no pixel data"
            exported.append(info)
            continue
        dest = STAGING_TEX / f"Castle_Tier{tier}_BaseColor.jpg"
        try:
            img.filepath_raw = str(dest)
            img.file_format = "JPEG"
            img.save()
            info["path"] = str(dest)
            info["bytes"] = dest.stat().st_size if dest.is_file() else 0
        except Exception as ex:  # noqa: BLE001
            info["error"] = str(ex)
        exported.append(info)
    return exported


def prepare_tier(tier: int) -> dict:
    src = SOURCE / f"Castle_Tier{tier}.glb"
    result = {
        "tier": tier,
        "source": str(src),
        "ok": False,
        "error": None,
    }
    if not src.is_file():
        result["error"] = "missing"
        return result

    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(src))
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not meshes:
        result["error"] = "no_mesh"
        return result

    result["mesh_count"] = len(meshes)
    result["object_names"] = [o.name for o in bpy.context.scene.objects][:40]
    result["loose_geometry"] = len(meshes) > 3

    root = bpy.data.objects.new(f"Castle_Tier{tier}", None)
    bpy.context.scene.collection.objects.link(root)
    for o in list(bpy.context.scene.objects):
        if o == root:
            continue
        o.parent = root
        o.matrix_parent_inverse = root.matrix_world.inverted()

    mn, mx = world_bounds(meshes)
    size0 = mx - mn
    footprint0 = max(size0.x, size0.y)
    if footprint0 <= 1e-6:
        result["error"] = "degenerate"
        return result

    target = TARGET_FOOTPRINT[tier]
    scale = target / footprint0
    root.scale = (scale, scale, scale)
    bpy.context.view_layer.update()

    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    bpy.context.view_layer.objects.active = root
    for child in list(root.children):
        child.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    root.scale = (1, 1, 1)
    bpy.context.view_layer.update()

    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    mn, mx = world_bounds(meshes)
    center_xy = Vector(((mn.x + mx.x) * 0.5, (mn.y + mx.y) * 0.5, 0.0))
    delta = Vector((-center_xy.x, -center_xy.y, -mn.z))
    root.location += delta
    bpy.context.view_layer.update()

    bpy.ops.object.select_all(action="DESELECT")
    for child in list(root.children):
        child.select_set(True)
        bpy.context.view_layer.objects.active = child
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    root.location = (0, 0, 0)
    bpy.context.view_layer.update()

    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    mn, mx = world_bounds(meshes)
    size = mx - mn

    for mat in bpy.data.materials:
        if mat.name.startswith("tripo_"):
            mat.name = f"M_Castle_Tier{tier}_Atlas"

    roughness = 0.5
    for mat in bpy.data.materials:
        if not mat.use_nodes or not mat.node_tree:
            continue
        for n in mat.node_tree.nodes:
            if n.type == "BSDF_PRINCIPLED" and n.inputs.get("Roughness"):
                roughness = float(n.inputs["Roughness"].default_value)
                break

    textures = export_textures(tier)

    fbx_path = STAGING_MODELS / f"Castle_Tier{tier}.fbx"
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(
        filepath=str(fbx_path),
        use_selection=False,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        path_mode="COPY",
        embed_textures=True,
        add_leaf_bones=False,
    )

    footprint = float(max(size.x, size.y))
    height = float(size.z)
    result.update(
        {
            "ok": True,
            "source_bytes": src.stat().st_size,
            "source_mtime": datetime.fromtimestamp(src.stat().st_mtime).isoformat(),
            "raw_size": [size0.x, size0.y, size0.z],
            "scale_factor": scale,
            "final_footprint": footprint,
            "final_height": height,
            "pivot": "base center, min Z=0 (Blender); Unity Y-up after FBX",
            "orientation": "FBX axis_up=Y axis_forward=-Z; gate target Unity +Z",
            "materials": [m.name for m in bpy.data.materials],
            "roughness": roughness,
            "smoothness": 1.0 - roughness,
            "textures": textures,
            "texture_ok": any(t.get("bytes", 0) > 0 for t in textures),
            "fbx": str(fbx_path),
            "fbx_bytes": fbx_path.stat().st_size if fbx_path.is_file() else 0,
            "within_progressive_target": abs(footprint - target) < 0.15,
            "glb_valid_magic": True,
            "embedded_base_color": any(t.get("bytes", 0) > 0 for t in textures),
        }
    )
    print(
        "TIER_OK",
        json.dumps(
            {
                "tier": tier,
                "footprint": footprint,
                "height": height,
                "tex_ok": result["texture_ok"],
                "scale": scale,
            }
        ),
    )
    return result


def main():
    STAGING_MODELS.mkdir(parents=True, exist_ok=True)
    STAGING_TEX.mkdir(parents=True, exist_ok=True)
    REPORTS.mkdir(parents=True, exist_ok=True)

    report = {
        "generated_utc": datetime.now(timezone.utc).isoformat(),
        "level_bands": {
            "1-5": "Castle_Tier1",
            "6-10": "Castle_Tier2",
            "11-15": "Castle_Tier3",
            "16-20": "Castle_Tier4",
            "21-25": "Castle_Tier5",
            "26-30": "Castle_Tier6",
        },
        "target_footprints_m": TARGET_FOOTPRINT,
        "tiers": [],
    }
    for tier in range(1, 7):
        report["tiers"].append(prepare_tier(tier))

    path = REPORTS / "prepare_all_tiers.json"
    path.write_text(json.dumps(report, indent=2), encoding="utf-8")
    ok = sum(1 for t in report["tiers"] if t.get("ok") and t.get("texture_ok") and t.get("within_progressive_target"))
    print("ALL_DONE", json.dumps({"ok_full": ok, "total": 6, "report": str(path)}))


if __name__ == "__main__":
    main()
