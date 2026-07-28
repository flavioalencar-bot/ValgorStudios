import bpy, json, math
from mathutils import Vector
from pathlib import Path

src = Path(r"C:\Valgor_Studio\production\City\Castle\source\Castle_Tier1.glb")
out = Path(r"C:\Valgor_Studio\production\City\Castle\reports\inspect_raw.json")

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src))

objs = [o for o in bpy.context.scene.objects if o.type in {"MESH", "EMPTY", "ARMATURE"}]
meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]

# world bounds
mn = Vector((1e9,1e9,1e9)); mx = Vector((-1e9,-1e9,-1e9))
for o in meshes:
    for corner in o.bound_box:
        w = o.matrix_world @ Vector(corner)
        mn.x=min(mn.x,w.x); mn.y=min(mn.y,w.y); mn.z=min(mn.z,w.z)
        mx.x=max(mx.x,w.x); mx.y=max(mx.y,w.y); mx.z=max(mx.z,w.z)

size = mx-mn
mats=[]
for m in bpy.data.materials:
    nodes = []
    if m.use_nodes and m.node_tree:
        for n in m.node_tree.nodes:
            if n.type=='TEX_IMAGE' and n.image:
                nodes.append(n.image.name)
    mats.append({"name": m.name, "images": nodes})

report = {
  "objects": [{"name":o.name,"type":o.type,"loc":list(o.location),"dims":list(o.dimensions)} for o in objs[:80]],
  "mesh_count": len(meshes),
  "bounds_min": list(mn),
  "bounds_max": list(mx),
  "size": list(size),
  "materials": mats[:40],
  "material_count": len(bpy.data.materials),
  "image_count": len(bpy.data.images),
  "images": [i.name for i in bpy.data.images][:40],
}
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps(report, indent=2), encoding="utf-8")
print("OK", json.dumps({"size":list(size),"meshes":len(meshes),"mats":len(bpy.data.materials),"imgs":len(bpy.data.images)}))
