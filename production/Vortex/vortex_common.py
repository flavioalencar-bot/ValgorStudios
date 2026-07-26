"""
VALGOR — shared constants for Vortex Blender production tools.
"""

from __future__ import annotations

from pathlib import Path

CHARACTER_ID = "HERO_VORTEX_000"
CHARACTER_NAME = "Vortex"
TARGET_HEIGHT_M = 2.05
HEIGHT_REF_NAME = "Vortex_HeightReference_2_05m"
RIG_OBJECT_NAME = "Vortex_Rig"
GROUND_NAME = "GroundReference"

REQUIRED_COLLECTIONS = [
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

REQUIRED_SOCKETS = [
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

REQUIRED_MATERIALS = [
    "MAT_Vortex_Skin",
    "MAT_Vortex_Hair",
    "MAT_Vortex_ArmorBlack",
    "MAT_Vortex_ArmorGold",
    "MAT_Vortex_Cloth",
    "MAT_Vortex_Eyes",
    "MAT_Vortex_Sword",
]

REQUIRED_TEXTURES = [
    "Vortex_Body_BaseColor.png",
    "Vortex_Body_Normal.png",
    "Vortex_Body_Mask.png",
    "Vortex_Armor_BaseColor.png",
    "Vortex_Armor_Normal.png",
    "Vortex_Armor_Mask.png",
    "Vortex_Cape_BaseColor.png",
    "Vortex_Cape_Normal.png",
    "Vortex_Cape_Mask.png",
    "Vortex_Weapon_BaseColor.png",
    "Vortex_Weapon_Normal.png",
    "Vortex_Weapon_Mask.png",
    "Vortex_Hair_BaseColor.png",
    "Vortex_Hair_Normal.png",
    "Vortex_Eyes_Emission.png",
]

HUMANOID_BONES_MIN = [
    "Hips",
    "Spine",
    "Chest",
    "UpperChest",
    "Neck",
    "Head",
    "LeftShoulder",
    "LeftUpperArm",
    "LeftLowerArm",
    "LeftHand",
    "RightShoulder",
    "RightUpperArm",
    "RightLowerArm",
    "RightHand",
    "LeftUpperLeg",
    "LeftLowerLeg",
    "LeftFoot",
    "LeftToes",
    "RightUpperLeg",
    "RightLowerLeg",
    "RightFoot",
    "RightToes",
]

SCENE_METADATA_KEYS = {
    "VALGOR_CHARACTER_ID": CHARACTER_ID,
    "VALGOR_CHARACTER_NAME": CHARACTER_NAME,
    "VALGOR_HEIGHT_M": TARGET_HEIGHT_M,
    "VALGOR_FORWARD_AXIS": "+Z",
    "VALGOR_UP_AXIS": "+Y",
    "VALGOR_TARGET_ENGINE": "Unity 6",
    "VALGOR_RIG_TYPE": "Humanoid",
}

# Technical / non-art objects that must NOT be treated as final hero mesh.
TECHNICAL_OBJECT_NAMES = {
    HEIGHT_REF_NAME,
    GROUND_NAME,
    RIG_OBJECT_NAME,
}

ROOT = Path(r"C:\Valgor_Studio\production\Vortex")
BLEND_PATH = ROOT / "Vortex_Production.blend"
EXPORT_DIR = ROOT / "export"
REPORTS_DIR = ROOT / "reports"
STAGING_DIR = ROOT / "unity_staging"
TEXTURES_DIR = ROOT / "textures"
SOURCE_DIR = ROOT / "source"

# Drop these exact names into source/ to trigger base import.
BASE_MODEL_CANDIDATES = (
    "Vortex_Base.glb",
    "Vortex_Base.fbx",
    "Vortex_Base.gltf",
)

# Scene custom props governing import / Unity gate
PROP_VISUAL_APPROVED = "VALGOR_VISUAL_APPROVED"
PROP_UNITY_EXPORT_ALLOWED = "VALGOR_UNITY_EXPORT_ALLOWED"
PROP_BASE_IMPORTED = "VALGOR_BASE_IMPORTED"
PROP_BASE_SOURCE_FILE = "VALGOR_BASE_SOURCE_FILE"
PROP_APPROVED_ART = "VALGOR_APPROVED_ART"
PROP_IMPORT_TAG = "VALGOR_IMPORT_TAG"
IMPORT_TAG_BASE = "vortex_base"

UNITY_MODELS_REL = (
    r"client\Assets\Valgor\Heroes\Characters\Vortex\Models"
)
UNITY_TEXTURES_REL = (
    r"client\Assets\Valgor\Heroes\Characters\Vortex\Textures"
)

LOD0_TRIS = (55000, 85000)
LOD1_TRIS = (25000, 40000)
LOD2_TRIS = (8000, 15000)
