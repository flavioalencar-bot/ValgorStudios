"""
VALGOR — Build Humanoid rig, minimal animation set, dragon sword, and re-export FBX.

Preserves approved Vortex_Base mesh/material identity (no remesh / no proportion rewrite).
"""

from __future__ import annotations

import math
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import bpy
from mathutils import Vector

from vortex_common import (
    EXPORT_DIR,
    HUMANOID_BONES_MIN,
    PROP_UNITY_EXPORT_ALLOWED,
    PROP_VISUAL_APPROVED,
    REPORTS_DIR,
    RIG_OBJECT_NAME,
    REQUIRED_SOCKETS,
)
from vortex_report import Report

MESH_NAME = "Vortex_Base"
SWORD_NAME = "Vortex_DragonSword"
FPS = 30

ANIMATIONS = {
    # name: (frames, builder_fn key)
    "Idle": 60,
    "Idle_Combat": 60,
    "Walk": 32,
    "Run": 24,
    "Turn_Left": 24,
    "Turn_Right": 24,
    "Attack_01": 28,
    "Attack_02": 32,
    "Heavy_Attack": 40,
    "Special_Power": 90,  # raise/plant sword; aura VFX lasts 10s in Unity
    "Hit_Front": 18,
    "Hit_Back": 18,
    "Stun": 48,
    "Victory": 60,
    "Defeat": 48,
    "Death": 60,
}


def _ensure_object_mode():
    if bpy.context.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")


def _deselect_all():
    bpy.ops.object.select_all(action="DESELECT")


def _mesh_bounds(obj):
    corners = [obj.matrix_world @ Vector(c) for c in obj.bound_box]
    xs = [v.x for v in corners]
    ys = [v.y for v in corners]
    zs = [v.z for v in corners]
    mn = Vector((min(xs), min(ys), min(zs)))
    mx = Vector((max(xs), max(ys), max(zs)))
    return mn, mx, mx - mn


def _clear_old_rig(report: Report):
    _ensure_object_mode()
    mesh = bpy.data.objects.get(MESH_NAME)
    if mesh is not None:
        # Remove armature modifiers pointing at old rig
        for mod in list(mesh.modifiers):
            if mod.type == "ARMATURE":
                mesh.modifiers.remove(mod)
        if mesh.parent is not None and mesh.parent.type == "ARMATURE":
            mesh.parent = None

    old = bpy.data.objects.get(RIG_OBJECT_NAME)
    if old is not None:
        arm_data = old.data
        bpy.data.objects.remove(old, do_unlink=True)
        if arm_data is not None:
            bpy.data.armatures.remove(arm_data, do_unlink=True)
        report.add("Removed placeholder armature", True)
    else:
        report.add("No previous armature to remove", True, level="warn")


def _create_humanoid_armature(mesh, report: Report):
    """Build Unity-named Humanoid bones fitted to mesh AABB (Z-up Blender)."""
    mn, mx, size = _mesh_bounds(mesh)
    h = max(size.z, 0.01)
    cx = (mn.x + mx.x) * 0.5
    cy = (mn.y + mx.y) * 0.5
    # Depth: prefer facing -Y in Blender so FBX -Z forward maps to Unity +Z consistently
    # with existing export settings.
    half_w = size.x * 0.5

    def Z(t):
        return mn.z + h * t

    def P(x, y, z):
        return Vector((cx + x, cy + y, z))

    arm_data = bpy.data.armatures.new("Vortex_RigData")
    arm_obj = bpy.data.objects.new(RIG_OBJECT_NAME, arm_data)
    bpy.context.scene.collection.objects.link(arm_obj)

    # Collection
    rig_col = bpy.data.collections.get("VORTEX_RIG")
    if rig_col is not None and arm_obj.name not in rig_col.objects:
        # unlink from scene root collection if needed
        for col in list(arm_obj.users_collection):
            if col != rig_col:
                col.objects.unlink(arm_obj)
        if arm_obj.name not in rig_col.objects:
            rig_col.objects.link(arm_obj)

    bpy.context.view_layer.objects.active = arm_obj
    arm_obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    eb = arm_data.edit_bones

    def add(name, head, tail, parent=None, connect=False):
        b = eb.new(name)
        b.head = head
        b.tail = tail
        if parent is not None:
            b.parent = eb[parent]
            b.use_connect = connect
        return b

    # Spine chain
    add("Hips", P(0, 0, Z(0.48)), P(0, 0, Z(0.55)))
    add("Spine", P(0, 0, Z(0.55)), P(0, 0, Z(0.62)), "Hips")
    add("Chest", P(0, 0, Z(0.62)), P(0, 0, Z(0.70)), "Spine")
    add("UpperChest", P(0, 0, Z(0.70)), P(0, 0, Z(0.78)), "Chest")
    add("Neck", P(0, 0, Z(0.78)), P(0, 0, Z(0.84)), "UpperChest")
    add("Head", P(0, 0, Z(0.84)), P(0, 0, Z(0.98)), "Neck")

    # Legs
    hip_x = half_w * 0.18
    add("LeftUpperLeg", P(hip_x, 0, Z(0.48)), P(hip_x, 0.02, Z(0.27)), "Hips")
    add("LeftLowerLeg", P(hip_x, 0.02, Z(0.27)), P(hip_x, 0.04, Z(0.08)), "LeftUpperLeg", True)
    add("LeftFoot", P(hip_x, 0.04, Z(0.08)), P(hip_x, 0.12, Z(0.02)), "LeftLowerLeg", True)
    add("LeftToes", P(hip_x, 0.12, Z(0.02)), P(hip_x, 0.18, Z(0.01)), "LeftFoot", True)

    add("RightUpperLeg", P(-hip_x, 0, Z(0.48)), P(-hip_x, 0.02, Z(0.27)), "Hips")
    add("RightLowerLeg", P(-hip_x, 0.02, Z(0.27)), P(-hip_x, 0.04, Z(0.08)), "RightUpperLeg", True)
    add("RightFoot", P(-hip_x, 0.04, Z(0.08)), P(-hip_x, 0.12, Z(0.02)), "RightLowerLeg", True)
    add("RightToes", P(-hip_x, 0.12, Z(0.02)), P(-hip_x, 0.18, Z(0.01)), "RightFoot", True)

    # Arms (A-pose-ish)
    sh_x = half_w * 0.42
    sh_z = Z(0.76)
    add("LeftShoulder", P(half_w * 0.12, 0, sh_z), P(sh_x * 0.55, 0, sh_z), "UpperChest")
    add("LeftUpperArm", P(sh_x * 0.55, 0, sh_z), P(sh_x * 1.05, 0.02, Z(0.62)), "LeftShoulder")
    add("LeftLowerArm", P(sh_x * 1.05, 0.02, Z(0.62)), P(sh_x * 1.25, 0.04, Z(0.50)), "LeftUpperArm", True)
    add("LeftHand", P(sh_x * 1.25, 0.04, Z(0.50)), P(sh_x * 1.38, 0.06, Z(0.46)), "LeftLowerArm", True)

    add("RightShoulder", P(-half_w * 0.12, 0, sh_z), P(-sh_x * 0.55, 0, sh_z), "UpperChest")
    add("RightUpperArm", P(-sh_x * 0.55, 0, sh_z), P(-sh_x * 1.05, 0.02, Z(0.62)), "RightShoulder")
    add("RightLowerArm", P(-sh_x * 1.05, 0.02, Z(0.62)), P(-sh_x * 1.25, 0.04, Z(0.50)), "RightUpperArm", True)
    add("RightHand", P(-sh_x * 1.25, 0.04, Z(0.50)), P(-sh_x * 1.38, 0.06, Z(0.46)), "RightLowerArm", True)

    # Simple fingers (improve Humanoid mapping)
    for side, hand, sign in (
        ("Left", "LeftHand", 1.0),
        ("Right", "RightHand", -1.0),
    ):
        base = eb[hand].tail.copy()
        for i, name in enumerate(["Thumb", "Index", "Middle", "Ring", "Little"]):
            hx = base.x + sign * 0.02 * (i - 2) * 0.5
            hy = base.y + 0.03
            hz = base.z - 0.01 * abs(i - 2) * 0.1
            add(
                f"{side}{name}",
                Vector((hx, hy, hz)),
                Vector((hx + sign * 0.04, hy + 0.03, hz)),
                hand,
            )

    # Aux cape + hair (non-humanoid extras)
    add("Cape_01", P(0, -0.05, Z(0.72)), P(0, -0.12, Z(0.55)), "UpperChest")
    add("Cape_02", P(0, -0.12, Z(0.55)), P(0, -0.18, Z(0.35)), "Cape_01", True)
    add("Cape_03", P(0, -0.18, Z(0.35)), P(0, -0.22, Z(0.18)), "Cape_02", True)
    add("Hair_01", P(0, -0.02, Z(0.92)), P(0, -0.08, Z(0.88)), "Head")
    add("Hair_02", P(0, -0.08, Z(0.88)), P(0, -0.14, Z(0.78)), "Hair_01", True)

    bpy.ops.object.mode_set(mode="OBJECT")
    arm_obj.location = Vector((0, 0, 0))

    bones = {b.name for b in arm_data.bones}
    missing = [b for b in HUMANOID_BONES_MIN if b not in bones]
    report.add(
        "Humanoid bone set complete",
        len(missing) == 0,
        f"bones={len(bones)} missing={missing}",
    )
    report.add("Aux cape/hair bones", all(n in bones for n in ("Cape_01", "Hair_01")), level="warn")
    bpy.context.scene["VALGOR_RIG_TYPE"] = "Humanoid"
    return arm_obj


def _skin_mesh(mesh, arm_obj, report: Report):
    _ensure_object_mode()
    _deselect_all()
    mesh.select_set(True)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj
    bpy.ops.object.parent_set(type="ARMATURE_AUTO")
    # Ensure armature modifier exists and uses vertex groups
    mod = None
    for m in mesh.modifiers:
        if m.type == "ARMATURE":
            mod = m
            break
    if mod is None:
        mod = mesh.modifiers.new("Armature", "ARMATURE")
    mod.object = arm_obj
    mod.use_vertex_groups = True
    vg_count = len(mesh.vertex_groups)
    report.add("Automatic skinning (ARMATURE_AUTO)", vg_count >= 10, f"vertex_groups={vg_count}")
    return vg_count


def _ensure_socket_empties(arm_obj, report: Report):
    """Parent socket empties to approximate bones for export hierarchy hints."""
    bone_map = {
        "Socket_RightHand": "RightHand",
        "Socket_LeftHand": "LeftHand",
        "Socket_BackWeapon": "UpperChest",
        "Socket_HipWeapon": "Hips",
        "Socket_HeadVFX": "Head",
        "Socket_ChestVFX": "Chest",
        "Socket_FootLeftVFX": "LeftFoot",
        "Socket_FootRightVFX": "RightFoot",
        "Socket_DragonLink": "UpperChest",
    }
    created = 0
    for sock_name in REQUIRED_SOCKETS:
        obj = bpy.data.objects.get(sock_name)
        if obj is None:
            obj = bpy.data.objects.new(sock_name, None)
            obj.empty_display_type = "PLAIN_AXES"
            obj.empty_display_size = 0.05
            bpy.context.scene.collection.objects.link(obj)
            created += 1
        bone = bone_map.get(sock_name)
        if bone and bone in arm_obj.data.bones:
            obj.parent = arm_obj
            obj.parent_type = "BONE"
            obj.parent_bone = bone
            obj.location = Vector((0, 0, 0))
    report.add("Sockets parented to bones", True, f"created={created}")


def _pose_bone(arm_obj, name, euler_xyz_deg, frame):
    pb = arm_obj.pose.bones.get(name)
    if pb is None:
        return
    pb.rotation_mode = "XYZ"
    pb.rotation_euler = (
        math.radians(euler_xyz_deg[0]),
        math.radians(euler_xyz_deg[1]),
        math.radians(euler_xyz_deg[2]),
    )
    pb.keyframe_insert(data_path="rotation_euler", frame=frame)


def _reset_pose(arm_obj):
    for pb in arm_obj.pose.bones:
        pb.rotation_mode = "XYZ"
        pb.rotation_euler = (0, 0, 0)
        pb.location = (0, 0, 0)
        pb.scale = (1, 1, 1)


def _insert_rest(arm_obj, frame):
    for pb in arm_obj.pose.bones:
        pb.rotation_mode = "XYZ"
        pb.keyframe_insert(data_path="rotation_euler", frame=frame)
        pb.keyframe_insert(data_path="location", frame=frame)


def _make_action(arm_obj, name: str, frames: int, bake_fn):
    _ensure_object_mode()
    bpy.context.view_layer.objects.active = arm_obj
    arm_obj.select_set(True)
    bpy.ops.object.mode_set(mode="POSE")
    _reset_pose(arm_obj)

    # Clear existing action with same name
    if name in bpy.data.actions:
        bpy.data.actions.remove(bpy.data.actions[name])

    action = bpy.data.actions.new(name=name)
    arm_obj.animation_data_create()
    arm_obj.animation_data.action = action

    bake_fn(arm_obj, frames)

    # Ensure action frame range
    action.use_frame_range = True
    action.frame_start = 1
    action.frame_end = frames

    bpy.ops.object.mode_set(mode="OBJECT")
    return action


def _anim_idle(arm, frames):
    for f in (1, frames // 2, frames):
        breath = 3.0 if f == frames // 2 else 0.0
        _pose_bone(arm, "Spine", (breath, 0, 0), f)
        _pose_bone(arm, "Chest", (breath * 0.5, 0, 0), f)
        _pose_bone(arm, "LeftUpperArm", (0, 0, 4 if f == frames // 2 else 0), f)
        _pose_bone(arm, "RightUpperArm", (0, 0, -4 if f == frames // 2 else 0), f)
        _pose_bone(arm, "Cape_01", (2 if f == frames // 2 else 0, 0, 0), f)
        _pose_bone(arm, "Hair_01", (1 if f == frames // 2 else 0, 0, 0), f)


def _anim_idle_combat(arm, frames):
    for f in (1, frames // 2, frames):
        mid = f == frames // 2
        _pose_bone(arm, "RightUpperArm", (-25, 0, -20), f)
        _pose_bone(arm, "RightLowerArm", (-15, 0, 0), f)
        _pose_bone(arm, "LeftUpperArm", (-10, 0, 15), f)
        _pose_bone(arm, "Spine", (2 if mid else 0, 0, 0), f)


def _anim_locomotion(arm, frames, amp, speed_scale=1.0):
    # Simple biped cycle using sine-like key poses
    keys = [1, frames // 4, frames // 2, (3 * frames) // 4, frames]
    signs = [0, 1, 0, -1, 0]
    for f, s in zip(keys, signs):
        _pose_bone(arm, "LeftUpperLeg", (amp * s, 0, 0), f)
        _pose_bone(arm, "RightUpperLeg", (-amp * s, 0, 0), f)
        _pose_bone(arm, "LeftLowerLeg", (amp * 0.6 * max(0, -s), 0, 0), f)
        _pose_bone(arm, "RightLowerLeg", (amp * 0.6 * max(0, s), 0, 0), f)
        _pose_bone(arm, "LeftUpperArm", (-amp * 0.5 * s, 0, 0), f)
        _pose_bone(arm, "RightUpperArm", (amp * 0.5 * s, 0, 0), f)
        _pose_bone(arm, "Hips", (0, 0, 2 * s * speed_scale), f)


def _anim_turn(arm, frames, direction: int):
    for f, yaw in ((1, 0), (frames // 2, 35 * direction), (frames, 0)):
        _pose_bone(arm, "Hips", (0, 0, yaw), f)
        _pose_bone(arm, "Spine", (0, 0, yaw * 0.4), f)


def _anim_attack(arm, frames, heavy=False):
    wind = frames // 3
    hit = (2 * frames) // 3
    power = 55 if heavy else 40
    _pose_bone(arm, "RightUpperArm", (-10, 0, -10), 1)
    _pose_bone(arm, "RightUpperArm", (-70, 10, -30), wind)
    _pose_bone(arm, "RightUpperArm", (20, -20, power), hit)
    _pose_bone(arm, "RightUpperArm", (-10, 0, -10), frames)
    _pose_bone(arm, "RightLowerArm", (-20, 0, 0), 1)
    _pose_bone(arm, "RightLowerArm", (-50, 0, 0), wind)
    _pose_bone(arm, "RightLowerArm", (-5, 0, 0), hit)
    _pose_bone(arm, "RightLowerArm", (-20, 0, 0), frames)
    _pose_bone(arm, "Spine", (0, 0, 0), 1)
    _pose_bone(arm, "Spine", (-8, 0, -10), wind)
    _pose_bone(arm, "Spine", (10, 0, 15), hit)
    _pose_bone(arm, "Spine", (0, 0, 0), frames)
    if heavy:
        _pose_bone(arm, "Hips", (0, 0, 0), 1)
        _pose_bone(arm, "Hips", (0, 0, -15), hit)
        _pose_bone(arm, "Hips", (0, 0, 0), frames)


def _anim_special(arm, frames):
    # Raise sword, plant, hold with aura pose
    _pose_bone(arm, "RightUpperArm", (-20, 0, -15), 1)
    _pose_bone(arm, "RightUpperArm", (-110, 0, -10), frames // 4)
    _pose_bone(arm, "RightUpperArm", (-90, 0, 0), frames // 2)
    _pose_bone(arm, "RightUpperArm", (-95, 0, 0), frames)
    _pose_bone(arm, "RightLowerArm", (-10, 0, 0), 1)
    _pose_bone(arm, "RightLowerArm", (-5, 0, 0), frames // 4)
    _pose_bone(arm, "RightLowerArm", (10, 0, 0), frames // 2)
    _pose_bone(arm, "RightLowerArm", (10, 0, 0), frames)
    _pose_bone(arm, "Head", (5, 0, 0), 1)
    _pose_bone(arm, "Head", (-10, 0, 0), frames // 2)
    _pose_bone(arm, "Head", (-8, 0, 0), frames)
    _pose_bone(arm, "Cape_01", (0, 0, 0), 1)
    _pose_bone(arm, "Cape_01", (8, 0, 0), frames // 2)
    _pose_bone(arm, "Cape_01", (6, 0, 0), frames)


def _anim_hit(arm, frames, front=True):
    pitch = -18 if front else 18
    _pose_bone(arm, "Spine", (0, 0, 0), 1)
    _pose_bone(arm, "Spine", (pitch, 0, 0), frames // 2)
    _pose_bone(arm, "Spine", (0, 0, 0), frames)
    _pose_bone(arm, "Head", (pitch * 0.5, 0, 0), frames // 2)
    _pose_bone(arm, "Head", (0, 0, 0), frames)


def _anim_stun(arm, frames):
    for f in (1, frames // 2, frames):
        mid = f == frames // 2
        _pose_bone(arm, "Spine", (8 if mid else 4, 0, 0), f)
        _pose_bone(arm, "Head", (-15 if mid else -8, 0, 10 if mid else -10), f)
        _pose_bone(arm, "LeftUpperArm", (-30, 0, 25), f)
        _pose_bone(arm, "RightUpperArm", (-30, 0, -25), f)


def _anim_victory(arm, frames):
    _pose_bone(arm, "RightUpperArm", (-20, 0, 0), 1)
    _pose_bone(arm, "RightUpperArm", (-140, 0, -20), frames // 2)
    _pose_bone(arm, "RightUpperArm", (-130, 0, -15), frames)
    _pose_bone(arm, "LeftUpperArm", (-20, 0, 0), 1)
    _pose_bone(arm, "LeftUpperArm", (-120, 0, 20), frames // 2)
    _pose_bone(arm, "Spine", (0, 0, 0), 1)
    _pose_bone(arm, "Spine", (-8, 0, 0), frames // 2)


def _anim_defeat(arm, frames):
    _pose_bone(arm, "Spine", (0, 0, 0), 1)
    _pose_bone(arm, "Spine", (25, 0, 0), frames)
    _pose_bone(arm, "Head", (0, 0, 0), 1)
    _pose_bone(arm, "Head", (30, 0, 0), frames)
    _pose_bone(arm, "LeftUpperArm", (0, 0, 0), 1)
    _pose_bone(arm, "LeftUpperArm", (15, 0, 20), frames)
    _pose_bone(arm, "RightUpperArm", (0, 0, 0), 1)
    _pose_bone(arm, "RightUpperArm", (15, 0, -20), frames)


def _anim_death(arm, frames):
    _pose_bone(arm, "Hips", (0, 0, 0), 1)
    _pose_bone(arm, "Hips", (70, 0, 0), frames)
    _pose_bone(arm, "Spine", (0, 0, 0), 1)
    _pose_bone(arm, "Spine", (20, 0, 0), frames // 2)
    _pose_bone(arm, "Head", (0, 0, 0), 1)
    _pose_bone(arm, "Head", (25, 0, 0), frames)


def _build_all_animations(arm_obj, report: Report):
    builders = {
        "Idle": lambda a, f: _anim_idle(a, f),
        "Idle_Combat": lambda a, f: _anim_idle_combat(a, f),
        "Walk": lambda a, f: _anim_locomotion(a, f, amp=28),
        "Run": lambda a, f: _anim_locomotion(a, f, amp=42, speed_scale=1.4),
        "Turn_Left": lambda a, f: _anim_turn(a, f, 1),
        "Turn_Right": lambda a, f: _anim_turn(a, f, -1),
        "Attack_01": lambda a, f: _anim_attack(a, f, heavy=False),
        "Attack_02": lambda a, f: _anim_attack(a, f, heavy=False),
        "Heavy_Attack": lambda a, f: _anim_attack(a, f, heavy=True),
        "Special_Power": lambda a, f: _anim_special(a, f),
        "Hit_Front": lambda a, f: _anim_hit(a, f, True),
        "Hit_Back": lambda a, f: _anim_hit(a, f, False),
        "Stun": lambda a, f: _anim_stun(a, f),
        "Victory": lambda a, f: _anim_victory(a, f),
        "Defeat": lambda a, f: _anim_defeat(a, f),
        "Death": lambda a, f: _anim_death(a, f),
    }

    bpy.context.scene.render.fps = FPS
    created = []
    for name, frames in ANIMATIONS.items():
        _make_action(arm_obj, name, frames, builders[name])
        created.append(name)
    report.add("Animation actions created", len(created) == 16, f"{len(created)}: {', '.join(created)}")

    # Leave Idle as active for preview
    if "Idle" in bpy.data.actions:
        arm_obj.animation_data.action = bpy.data.actions["Idle"]


def _create_or_update_sword(report: Report):
    _ensure_object_mode()
    existing = bpy.data.objects.get(SWORD_NAME)
    if existing is not None:
        bpy.data.objects.remove(existing, do_unlink=True)

    # Stylized dragon sword from primitives (separate weapon asset — not body remesh)
    parts = []

    def prim(op, name, scale, loc, rot=(0, 0, 0)):
        op()
        obj = bpy.context.active_object
        obj.name = name
        obj.scale = scale
        obj.location = loc
        obj.rotation_euler = tuple(math.radians(r) for r in rot)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        parts.append(obj)
        return obj

    _deselect_all()
    prim(lambda: bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=1, depth=1), "sw_handle", (0.03, 0.03, 0.18), (0, 0, 0.09))
    prim(lambda: bpy.ops.mesh.primitive_torus_add(major_radius=0.07, minor_radius=0.015), "sw_guard", (1, 0.35, 1), (0, 0, 0.19), (90, 0, 0))
    prim(lambda: bpy.ops.mesh.primitive_cube_add(), "sw_blade", (0.035, 0.012, 0.55), (0, 0, 0.50))
    prim(lambda: bpy.ops.mesh.primitive_cone_add(vertices=8), "sw_tip", (0.035, 0.035, 0.08), (0, 0, 0.80))
    prim(lambda: bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=8), "sw_pommel", (0.04, 0.04, 0.04), (0, 0, 0.0))

    # Gold material hint
    mat = bpy.data.materials.get("MAT_Vortex_Sword")
    if mat is None:
        mat = bpy.data.materials.new("MAT_Vortex_Sword")
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        if bsdf:
            bsdf.inputs["Base Color"].default_value = (0.55, 0.55, 0.62, 1)
            if "Metallic" in bsdf.inputs:
                bsdf.inputs["Metallic"].default_value = 0.9

    _deselect_all()
    for p in parts:
        p.select_set(True)
        if p.data.materials:
            p.data.materials[0] = mat
        else:
            p.data.materials.append(mat)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    sword = bpy.context.active_object
    sword.name = SWORD_NAME
    # Pivot at grip (handle base)
    bpy.context.scene.cursor.location = Vector((0, 0, 0.05))
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    sword.location = Vector((0, 0, 0))

    # Collection
    wcol = bpy.data.collections.get("VORTEX_WEAPON")
    if wcol is not None:
        for col in list(sword.users_collection):
            if col != wcol:
                col.objects.unlink(sword)
        if sword.name not in wcol.objects:
            wcol.objects.link(sword)

    report.add("Dragon sword mesh created", sword is not None, f"tris≈{len(sword.data.polygons)}")
    return sword


def _export_character(arm_obj, mesh, report: Report) -> Path:
    EXPORT_DIR.mkdir(parents=True, exist_ok=True)
    out = EXPORT_DIR / "Vortex_LOD0.fbx"

    # Push all actions into NLA so FBX export includes them
    if arm_obj.animation_data is None:
        arm_obj.animation_data_create()
    track_names = {t.name for t in arm_obj.animation_data.nla_tracks}
    for action in bpy.data.actions:
        if action.name not in ANIMATIONS:
            continue
        tname = f"nla_{action.name}"
        if tname in track_names:
            # remove old
            for t in list(arm_obj.animation_data.nla_tracks):
                if t.name == tname:
                    arm_obj.animation_data.nla_tracks.remove(t)
        track = arm_obj.animation_data.nla_tracks.new()
        track.name = tname
        track.strips.new(action.name, int(action.frame_start), action)

    _ensure_object_mode()
    _deselect_all()
    mesh.select_set(True)
    arm_obj.select_set(True)
    for sock in REQUIRED_SOCKETS:
        o = bpy.data.objects.get(sock)
        if o is not None:
            o.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj

    bpy.ops.export_scene.fbx(
        filepath=str(out),
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        bake_anim=True,
        bake_anim_use_all_actions=True,
        bake_anim_use_nla_strips=True,
        bake_anim_force_startend_keying=True,
        bake_anim_step=1.0,
        bake_anim_simplify_factor=0.0,
        path_mode="COPY",
        embed_textures=True,
        armature_nodetype="NULL",
        primary_bone_axis="Y",
        secondary_bone_axis="X",
    )
    report.add("Exported Vortex_LOD0.fbx (rig+anims)", out.is_file(), str(out))
    return out


def _export_sword(sword, report: Report) -> Path:
    EXPORT_DIR.mkdir(parents=True, exist_ok=True)
    out = EXPORT_DIR / "Vortex_DragonSword.fbx"
    _ensure_object_mode()
    _deselect_all()
    sword.select_set(True)
    bpy.context.view_layer.objects.active = sword
    bpy.ops.export_scene.fbx(
        filepath=str(out),
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="COPY",
        embed_textures=True,
    )
    report.add("Exported Vortex_DragonSword.fbx", out.is_file(), str(out))
    return out


def run():
    report = Report("Vortex — Rig / Animations / Weapon")
    scene = bpy.context.scene
    if not bool(scene.get(PROP_VISUAL_APPROVED, False)):
        report.add("Visual approval required", False, "VALGOR_VISUAL_APPROVED is false")
        report.write(REPORTS_DIR / "16_rig_animate_weapon.txt")
        print(report.to_text())
        return report

    # Keep export allowed
    scene[PROP_UNITY_EXPORT_ALLOWED] = True

    mesh = bpy.data.objects.get(MESH_NAME)
    report.add(f"Mesh `{MESH_NAME}` present", mesh is not None)
    if mesh is None:
        report.write(REPORTS_DIR / "16_rig_animate_weapon.txt")
        print(report.to_text())
        return report

    _clear_old_rig(report)
    arm = _create_humanoid_armature(mesh, report)
    _skin_mesh(mesh, arm, report)
    _ensure_socket_empties(arm, report)
    _build_all_animations(arm, report)
    sword = _create_or_update_sword(report)
    char_fbx = _export_character(arm, mesh, report)
    sword_fbx = _export_sword(sword, report)

    # Save blend
    blend = ROOT / "Vortex_Production.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    report.add("Saved Vortex_Production.blend", blend.is_file(), str(blend))
    report.notes.append(
        "Mesh identity preserved (no remesh). Procedural Humanoid rig + minimal gameplay clips + separate sword."
    )
    report.notes.append(f"Character FBX: {char_fbx}")
    report.notes.append(f"Sword FBX: {sword_fbx}")
    out = report.write(REPORTS_DIR / "16_rig_animate_weapon.txt")
    print(report.to_text())
    print(f"Wrote {out}")
    return report


if __name__ == "__main__":
    run()
