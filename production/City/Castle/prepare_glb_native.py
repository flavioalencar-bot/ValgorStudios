"""Normalize Castle Tier GLBs for Unity glTFast: scale+pivot, preserve mesh/UV/materials.

Exports GLB only (no FBX). Avoids Blender FBX V-flip / UV corruption.
"""
from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(r"C:\Valgor_Studio\production\City\Castle")
SOURCE = ROOT / "source"
STAGING_GLB = ROOT / "unity_staging" / "Glb"
STAGING_TEX = ROOT / "unity_staging" / "Textures"
REPORTS = ROOT / "reports"

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
            mn.x, mn.y, mn.z = min(mn.x, w.x), min(mn.y, w.y), min(mn.z, w.z)
            mx.x, mx.y, mx.z = max(mx.x, w.x), max(mx.y, w.y), max(mx.z, w.z)
    return mn, mx


def export_textures(tier: int) -> list:
    out = []
    for img in bpy.data.images:
        if not (img.packed_file or img.has_data):
            continue
        dest = STAGING_TEX / f"Castle_Tier{tier}_BaseColor.jpg"
        try:
            img.filepath_raw = str(dest)
            img.file_format = "JPEG"
            img.save()
            out.append({"name": img.name, "path": str(dest), "bytes": dest.stat().st_size, "size": list(img.size)})
        except Exception as ex:  # noqa: BLE001
            out.append({"name": img.name, "error": str(ex)})
    return out


def prepare_tier(tier: int) -> dict:
    src = SOURCE / f"Castle_Tier{tier}.glb"
    result = {"tier": tier, "source": str(src), "ok": False}
    if not src.is_file():
        result["error"] = "missing"
        return result

    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(src))
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not meshes:
        result["error"] = "no_mesh"
        return result

    # Record pre-state
    mat_names = [m.name for m in bpy.data.materials]
    uv_ok = all(bool(o.data.uv_layers) for o in meshes)

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
    scale = TARGET_FOOTPRINT[tier] / footprint0
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
    delta = Vector((-(mn.x + mx.x) * 0.5, -(mn.y + mx.y) * 0.5, -mn.z))
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

    # Keep original material names; only rename tripo_* for clarity
    for mat in bpy.data.materials:
        if mat.name.startswith("tripo_"):
            mat.name = f"M_Castle_Tier{tier}_Mat0"

    textures = export_textures(tier)
    glb_path = STAGING_GLB / f"Castle_Tier{tier}.glb"
    bpy.ops.export_scene.gltf(
        filepath=str(glb_path),
        export_format="GLB",
        export_apply=True,
        export_texcoords=True,
        export_normals=True,
        export_materials="EXPORT",
        export_image_format="AUTO",
    )

    result.update(
        {
            "ok": True,
            "uv_ok_pre": uv_ok,
            "materials_pre": mat_names,
            "materials_post": [m.name for m in bpy.data.materials],
            "mesh_count": len(meshes),
            "scale_factor": scale,
            "final_footprint": float(max(size.x, size.y)),
            "final_height": float(size.z),
            "textures": textures,
            "glb": str(glb_path),
            "glb_bytes": glb_path.stat().st_size if glb_path.is_file() else 0,
            "importer_target": "Unity glTFast (com.unity.cloud.gltfast) — native GLB, no FBX",
            "note": "Mesh/UV/BaseColor preserved; only uniform scale + base pivot applied",
        }
    )
    print("GLB_OK", json.dumps({"tier": tier, "fp": result["final_footprint"], "h": result["final_height"], "bytes": result["glb_bytes"]}))
    return result


def main():
    STAGING_GLB.mkdir(parents=True, exist_ok=True)
    STAGING_TEX.mkdir(parents=True, exist_ok=True)
    report = {"generated_utc": datetime.now(timezone.utc).isoformat(), "tiers": []}
    for tier in range(1, 7):
        report["tiers"].append(prepare_tier(tier))
    path = REPORTS / "prepare_glb_native.json"
    path.write_text(json.dumps(report, indent=2), encoding="utf-8")
    ok = sum(1 for t in report["tiers"] if t.get("ok"))
    print("ALL_GLB", json.dumps({"ok": ok, "report": str(path)}))


if __name__ == "__main__":
    main()
