"""
Run all Vortex Blender validations + pending report + safe export + Unity staging prep.

Usage (batch):
  blender -b Vortex_Production.blend --python run_all_validations.py

Or from Blender Scripting: open and Run Script.
"""

from __future__ import annotations

import sys
from pathlib import Path

# Ensure this folder is importable when Blender runs the file
ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from validate_scene import validate_scene
from validate_scale_pivot import validate_scale_pivot
from validate_humanoid_rig import validate_humanoid_rig
from validate_sockets import validate_sockets
from validate_materials_textures import validate_materials_textures
from export_fbx import export_fbx
from generate_pending_report import generate_pending_report
from prepare_unity_import import prepare_unity_import
from vortex_common import REPORTS_DIR, BLEND_PATH
from vortex_report import Report


def main() -> int:
    REPORTS_DIR.mkdir(parents=True, exist_ok=True)

    master = Report("Vortex — Full Automatic Validation")
    master.notes.append(f"Blend: {BLEND_PATH}")

    results = [
        ("01_validate_scene.txt", validate_scene()),
        ("02_validate_scale_pivot.txt", validate_scale_pivot()),
        ("03_validate_humanoid_rig.txt", validate_humanoid_rig()),
        ("04_validate_sockets.txt", validate_sockets()),
        ("05_validate_materials_textures.txt", validate_materials_textures()),
        ("06_export_fbx.txt", export_fbx()),
        ("07_pending_report.txt", generate_pending_report()),
        ("08_prepare_unity_import.txt", prepare_unity_import()),
    ]

    hard_fail = 0
    for filename, r in results:
        path = r.write(REPORTS_DIR / filename)
        master.add(
            r.title,
            r.failed == 0,
            f"FAIL={r.failed} WARN={r.warnings} → {path.name}",
            level="error" if r.failed else "info",
        )
        hard_fail += r.failed
        print(r.to_text())
        print("-" * 60)

    # High-level confirmation checklist requested by user
    from validate_scene import validate_scene as _vs
    from validate_scale_pivot import validate_scale_pivot as _vsp
    from validate_humanoid_rig import validate_humanoid_rig as _vr
    from validate_sockets import validate_sockets as _vso
    from validate_materials_textures import validate_materials_textures as _vm
    import bpy

    cols = {c.name for c in bpy.data.collections}
    master.notes.append(
        "CONFIRM collections: "
        + (
            "OK"
            if {"VORTEX_MODEL", "VORTEX_LOD0", "VORTEX_LOD1", "VORTEX_LOD2", "VORTEX_SOCKETS", "VORTEX_RIG"}.issubset(cols)
            else f"present={sorted(cols)}"
        )
    )
    master.notes.append(
        f"CONFIRM height metadata: {bpy.context.scene.get('VALGOR_HEIGHT_M')} "
        f"/ character={bpy.context.scene.get('VALGOR_CHARACTER_ID')}"
    )
    master.notes.append(
        f"CONFIRM armature: {'Vortex_Rig' in bpy.data.objects}"
    )
    master.notes.append(
        "CONFIRM LOD collections empty of art until modeled — expected."
    )

    out = master.write(REPORTS_DIR / "00_full_validation.txt")
    print(master.to_text())
    print(f"MASTER REPORT: {out}")
    # Exit 0 even with artistic warnings; non-zero only if scaffold hard-fails
    return 1 if hard_fail > 0 else 0


if __name__ == "__main__":
    raise SystemExit(main())
