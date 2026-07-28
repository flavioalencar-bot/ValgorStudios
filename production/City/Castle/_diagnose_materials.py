import bpy, json, os
from pathlib import Path

# Prefer newest source
src = Path(r"C:\Valgor_Studio\production\City\Castle\source\Castle_Tier1.glb")
tex_out = Path(r"C:\Valgor_Studio\production\City\Castle\unity_staging\Textures")
report_path = Path(r"C:\Valgor_Studio\production\City\Castle\reports\materials_diagnosis.json")
tex_out.mkdir(parents=True, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src))

diag = {
  "source": str(src),
  "source_bytes": src.stat().st_size,
  "materials": [],
  "images": [],
  "meshes": [],
}

for img in bpy.data.images:
    info = {
      "name": img.name,
      "size": list(img.size) if img.size else None,
      "channels": img.channels,
      "colorspace": img.colorspace_settings.name if img.colorspace_settings else None,
      "filepath": img.filepath,
      "packed": bool(img.packed_file),
      "has_data": bool(img.has_data),
    }
    # unpack/save
    safe = "".join(c if c.isalnum() or c in "._-" else "_" for c in img.name)
    if not any(safe.lower().endswith(ext) for ext in (".png", ".jpg", ".jpeg", ".webp", ".tga")):
        safe += ".png"
    dest = tex_out / safe
    try:
        if img.packed_file or img.has_data:
            img.filepath_raw = str(dest)
            img.file_format = "PNG"
            img.save()
            info["exported"] = str(dest)
            info["exported_bytes"] = dest.stat().st_size if dest.is_file() else 0
        else:
            info["exported"] = None
            info["export_error"] = "no pixel data"
    except Exception as e:
        info["exported"] = None
        info["export_error"] = str(e)
    diag["images"].append(info)

for mat in bpy.data.materials:
    m = {"name": mat.name, "use_nodes": bool(getattr(mat, "use_nodes", False)), "sockets": {}}
    if mat.use_nodes and mat.node_tree:
        # Principled BSDF
        principled = None
        for n in mat.node_tree.nodes:
            if n.type == "BSDF_PRINCIPLED":
                principled = n
                break
        if principled:
            for sock_name in (
                "Base Color", "Metallic", "Roughness", "Specular IOR Level",
                "Normal", "Alpha", "Emission Color", "Emission Strength",
                "Specular", "Coat Weight"
            ):
                sock = principled.inputs.get(sock_name)
                if sock is None:
                    continue
                linked = None
                if sock.is_linked:
                    from_node = sock.links[0].from_node
                    linked = {"node": from_node.name, "type": from_node.type}
                    if from_node.type == "TEX_IMAGE" and from_node.image:
                        linked["image"] = from_node.image.name
                    elif from_node.type == "NORMAL_MAP" and from_node.inputs.get("Color") and from_node.inputs["Color"].is_linked:
                        srcn = from_node.inputs["Color"].links[0].from_node
                        linked["normal_from"] = srcn.name
                        if srcn.type == "TEX_IMAGE" and srcn.image:
                            linked["image"] = srcn.image.name
                default = None
                try:
                    default = list(sock.default_value) if hasattr(sock.default_value, "__iter__") else float(sock.default_value)
                except Exception:
                    default = str(sock.default_value)
                m["sockets"][sock_name] = {"linked": linked, "default": default}
        # all image nodes
        m["tex_nodes"] = []
        for n in mat.node_tree.nodes:
            if n.type == "TEX_IMAGE":
                m["tex_nodes"].append({
                    "node": n.name,
                    "image": n.image.name if n.image else None,
                    "colorspace": n.image.colorspace_settings.name if n.image else None,
                })
    diag["materials"].append(m)

for o in bpy.context.scene.objects:
    if o.type != "MESH":
        continue
    slots = []
    for i, s in enumerate(o.material_slots):
        slots.append({"index": i, "material": s.material.name if s.material else None})
    diag["meshes"].append({"name": o.name, "slots": slots, "materials_count": len(o.data.materials)})

report_path.write_text(json.dumps(diag, indent=2), encoding="utf-8")
print("DIAG_OK", json.dumps({
  "mats": len(diag["materials"]),
  "images": len(diag["images"]),
  "exported": [i.get("exported") for i in diag["images"]],
}))
