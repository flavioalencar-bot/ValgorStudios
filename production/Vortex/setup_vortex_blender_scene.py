import bpy
from mathutils import Vector

# VALGOR - Vortex 3D production scene setup
# Run in Blender's Scripting workspace.

# Clean scene
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)

# Collections
collection_names = [
    "VORTEX_MODEL",
    "VORTEX_ARMOR",
    "VORTEX_CAPE",
    "VORTEX_WEAPON",
    "VORTEX_RIG",
    "VORTEX_SOCKETS",
    "VORTEX_LOD0",
    "VORTEX_LOD1",
    "VORTEX_LOD2",
]
collections = {}
for name in collection_names:
    col = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(col)
    collections[name] = col

# Scale reference: 2.05m
bpy.ops.mesh.primitive_cube_add(location=(0, 0, 1.025), scale=(0.05, 0.05, 1.025))
scale_ref = bpy.context.object
scale_ref.name = "Vortex_HeightReference_2_05m"

# Ground
bpy.ops.mesh.primitive_plane_add(size=4, location=(0, 0, 0))
ground = bpy.context.object
ground.name = "GroundReference"

# Root armature placeholder
bpy.ops.object.armature_add(enter_editmode=False, location=(0, 0, 0))
arm = bpy.context.object
arm.name = "Vortex_Rig"
arm.data.name = "Vortex_RigData"

# Sockets as empties
socket_names = [
    "Socket_RightHand",
    "Socket_LeftHand",
    "Socket_BackWeapon",
    "Socket_HipWeapon",
    "Socket_HeadVFX",
    "Socket_ChestVFX",
    "Socket_FootLeftVFX",
    "Socket_FootRightVFX",
    "Socket_DragonLink",
]

socket_positions = {
    "Socket_RightHand": (0.45, 0.0, 1.25),
    "Socket_LeftHand": (-0.45, 0.0, 1.25),
    "Socket_BackWeapon": (0.0, -0.18, 1.45),
    "Socket_HipWeapon": (0.35, 0.0, 0.95),
    "Socket_HeadVFX": (0.0, 0.0, 1.95),
    "Socket_ChestVFX": (0.0, 0.0, 1.45),
    "Socket_FootLeftVFX": (-0.16, 0.0, 0.05),
    "Socket_FootRightVFX": (0.16, 0.0, 0.05),
    "Socket_DragonLink": (0.0, -0.25, 1.55),
}

for name in socket_names:
    empty = bpy.data.objects.new(name, None)
    empty.empty_display_type = 'PLAIN_AXES'
    empty.empty_display_size = 0.08
    empty.location = socket_positions[name]
    collections["VORTEX_SOCKETS"].objects.link(empty)

# Material placeholders
materials = [
    ("MAT_Vortex_Skin", (0.22, 0.10, 0.06, 1.0)),
    ("MAT_Vortex_Hair", (0.01, 0.01, 0.01, 1.0)),
    ("MAT_Vortex_ArmorBlack", (0.015, 0.015, 0.015, 1.0)),
    ("MAT_Vortex_ArmorGold", (0.55, 0.28, 0.04, 1.0)),
    ("MAT_Vortex_Cloth", (0.01, 0.01, 0.015, 1.0)),
    ("MAT_Vortex_Eyes", (0.35, 0.02, 0.01, 1.0)),
    ("MAT_Vortex_Sword", (0.08, 0.02, 0.01, 1.0)),
]

for name, color in materials:
    mat = bpy.data.materials.new(name)
    mat.use_fake_user = True  # keep placeholders after save (unused materials otherwise purge)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = color
        if "Metallic" in bsdf.inputs:
            bsdf.inputs["Metallic"].default_value = 0.7 if "Armor" in name or "Sword" in name else 0.0
        if "Roughness" in bsdf.inputs:
            bsdf.inputs["Roughness"].default_value = 0.35

# Scene metadata
scene = bpy.context.scene
scene["VALGOR_CHARACTER_ID"] = "HERO_VORTEX_000"
scene["VALGOR_CHARACTER_NAME"] = "Vortex"
scene["VALGOR_HEIGHT_M"] = 2.05
scene["VALGOR_FORWARD_AXIS"] = "+Z"
scene["VALGOR_UP_AXIS"] = "+Y"
scene["VALGOR_TARGET_ENGINE"] = "Unity 6"
scene["VALGOR_RIG_TYPE"] = "Humanoid"

# Save helper text
text = bpy.data.texts.new("VORTEX_EXPORT_CHECKLIST")
text.write("""VORTEX EXPORT CHECKLIST
- Apply transforms
- Pivot at feet
- Height 2.05m
- Humanoid rig
- Required sockets
- LOD0/LOD1/LOD2
- FBX: -Z Forward, Y Up
- Disable Add Leaf Bones
- Export selected objects only
""")

print("Valgor Vortex production scene initialized.")
