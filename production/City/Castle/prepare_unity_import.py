"""Prepare Castle_Tier1 for Unity: scale, pivot, orientation, FBX+textures staging."""
from __future__ import annotations

import json
import shutil
from datetime import datetime, timezone
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(r"C:\Valgor_Studio\production\City\Castle")
SRC = ROOT / "source" / "Castle_Tier1.glb"
STAGING_MODELS = ROOT / "unity_staging" / "Models"
STAGING_TEX = ROOT / "unity_staging" / "Textures"
REPORTS = ROOT / "reports"
MANIFEST = ROOT / "unity_staging" / "unity_import_manifest.json"

# Target footprint (XZ in Blender = XY after Unity axis convert) mid of 5.5–9.0
TARGET_FOOTPRINT = 7.5
HEIGHT_MAX = 12.0


def world_bounds(meshes: list):
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


def main() -> None:
    if not SRC.is_file():
        raise SystemExit(f"Missing source: {SRC}")

    STAGING_MODELS.mkdir(parents=True, exist_ok=True)
    STAGING_TEX.mkdir(parents=True, exist_ok=True)
    REPORTS.mkdir(parents=True, exist_ok=True)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(SRC))

    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not meshes:
        raise SystemExit("No mesh in GLB")

    # Single root empty for transform control
    root = bpy.data.objects.new("Castle_Tier1", None)
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
        raise SystemExit("Degenerate footprint")

    scale = TARGET_FOOTPRINT / footprint0
    root.scale = (scale, scale, scale)
    bpy.context.view_layer.update()

    # Re-parent applied: apply scale on children via root
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    bpy.context.view_layer.objects.active = root
    # Apply scale to children by making root identity after applying
    for child in list(root.children):
        child.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    root.scale = (1, 1, 1)
    bpy.context.view_layer.update()

    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    mn, mx = world_bounds(meshes)
    # Pivot: base center on ground (Blender Z-up). Center X/Y, min Z → 0.
    center_xy = Vector(((mn.x + mx.x) * 0.5, (mn.y + mx.y) * 0.5, 0.0))
    delta = Vector((-center_xy.x, -center_xy.y, -mn.z))
    root.location += delta
    bpy.context.view_layer.update()

    # Apply location into mesh data so export is clean
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

    # Orientation note: City expects gate toward Unity +Z.
    # Tripo castle often faces -Y in Blender (→ Unity +Z with standard FBX).
    # Apply +90° Z if gate appears on +X after first QA; default: rotate so
    # longest "front" heuristic keeps model as authored (no extra yaw).
    orientation = {
        "blender_up": "Z",
        "unity_export": "axis_up=Y axis_forward=-Z",
        "gate_target_unity": "+Z",
        "extra_yaw_deg": 0,
        "note": "If gate faces wrong way in City, set yaw 90/180 in loader or re-run with yaw.",
    }

    # Export packed textures
    images = []
    for img in bpy.data.images:
        if not img.has_data:
            continue
        safe = "".join(c if c.isalnum() or c in "._-" else "_" for c in img.name)
        if not safe.lower().endswith((".png", ".jpg", ".jpeg")):
            safe += ".png"
        dest = STAGING_TEX / safe
        try:
            img.filepath_raw = str(dest)
            img.file_format = "PNG" if safe.lower().endswith(".png") else "JPEG"
            img.save()
            images.append(str(dest))
        except Exception as ex:  # noqa: BLE001
            images.append(f"FAIL:{img.name}:{ex}")

    # Rename material for convention
    for m in bpy.data.materials:
        if m.name.startswith("tripo_"):
            m.name = "M_Castle_Atlas"

    fbx_path = STAGING_MODELS / "Castle_Tier1.fbx"
    glb_path = STAGING_MODELS / "Castle_Tier1.glb"

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
    bpy.ops.export_scene.gltf(
        filepath=str(glb_path),
        export_format="GLB",
        export_apply=True,
    )

    height = float(size.z)
    footprint = float(max(size.x, size.y))
    report = {
        "generated_utc": datetime.now(timezone.utc).isoformat(),
        "source": str(SRC),
        "source_bytes": SRC.stat().st_size,
        "raw_size_blender": [size0.x, size0.y, size0.z],
        "scale_factor": scale,
        "final_size_blender_xyz": [size.x, size.y, size.z],
        "final_footprint": footprint,
        "final_height": height,
        "pivot": "base center, min Z = 0 (Blender); Unity Y-up after FBX",
        "orientation": orientation,
        "within_footprint_rules": 5.5 <= footprint <= 9.0,
        "within_height_rules": height <= HEIGHT_MAX,
        "materials": [{"name": m.name, "images": [
            n.image.name for n in (m.node_tree.nodes if m.use_nodes and m.node_tree else [])
            if getattr(n, "type", "") == "TEX_IMAGE" and n.image
        ]} for m in bpy.data.materials],
        "textures_exported": images,
        "staging_fbx": str(fbx_path),
        "staging_glb": str(glb_path),
        "mesh_count": len(meshes),
    }
    (REPORTS / "prepare_unity_import.json").write_text(
        json.dumps(report, indent=2), encoding="utf-8"
    )

    if MANIFEST.is_file():
        data = json.loads(MANIFEST.read_text(encoding="utf-8"))
        data["blocked"] = False
        data["status"] = "STAGED — ready for Unity import"
        data["scale_applied"] = {
            "factor": scale,
            "footprint": footprint,
            "height": height,
            "pivot": report["pivot"],
        }
        data["staging_files"] = [str(fbx_path), str(glb_path)]
        data["materials_found"] = [m.name for m in bpy.data.materials]
        MANIFEST.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    print("PREPARE_OK", json.dumps({
        "scale": scale,
        "footprint": footprint,
        "height": height,
        "fbx": str(fbx_path),
        "fbx_bytes": fbx_path.stat().st_size if fbx_path.is_file() else 0,
    }))


if __name__ == "__main__":
    main()
