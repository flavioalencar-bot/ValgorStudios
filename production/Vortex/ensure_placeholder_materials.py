"""
Restore provisional datablocks that Blender may purge when unused (no fake user).
Does NOT create a final hero mesh from primitives.
"""

from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import bpy

from vortex_common import REQUIRED_MATERIALS, BLEND_PATH


_COLORS = {
    "MAT_Vortex_Skin": (0.22, 0.10, 0.06, 1.0),
    "MAT_Vortex_Hair": (0.01, 0.01, 0.01, 1.0),
    "MAT_Vortex_ArmorBlack": (0.015, 0.015, 0.015, 1.0),
    "MAT_Vortex_ArmorGold": (0.55, 0.28, 0.04, 1.0),
    "MAT_Vortex_Cloth": (0.01, 0.01, 0.015, 1.0),
    "MAT_Vortex_Eyes": (0.35, 0.02, 0.01, 1.0),
    "MAT_Vortex_Sword": (0.08, 0.02, 0.01, 1.0),
}


def ensure_placeholder_materials() -> list[str]:
    created = []
    for name in REQUIRED_MATERIALS:
        mat = bpy.data.materials.get(name)
        if mat is None:
            mat = bpy.data.materials.new(name)
            created.append(name)
        mat.use_fake_user = True
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        color = _COLORS.get(name, (0.5, 0.5, 0.5, 1.0))
        if bsdf and "Base Color" in bsdf.inputs:
            bsdf.inputs["Base Color"].default_value = color
            if "Metallic" in bsdf.inputs:
                bsdf.inputs["Metallic"].default_value = (
                    0.7 if ("Armor" in name or "Sword" in name) else 0.0
                )
            if "Roughness" in bsdf.inputs:
                bsdf.inputs["Roughness"].default_value = 0.35
    return created


def main() -> None:
    created = ensure_placeholder_materials()
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    print(f"ensure_placeholder_materials: created={created}")
    print(f"Saved {BLEND_PATH}")


if __name__ == "__main__":
    main()
