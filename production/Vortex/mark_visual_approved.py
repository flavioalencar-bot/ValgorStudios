"""
Mark the current Vortex base import as visually approved.

ONLY run this after human visual review against the approved artboard.

Sets:
  VALGOR_VISUAL_APPROVED = True
  VALGOR_UNITY_EXPORT_ALLOWED = True
  VALGOR_APPROVED_ART = True on imported base objects

Usage:
  blender -b Vortex_Production.blend --python mark_visual_approved.py
"""

from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import bpy

from vortex_common import (
    BLEND_PATH,
    IMPORT_TAG_BASE,
    PROP_APPROVED_ART,
    PROP_IMPORT_TAG,
    PROP_UNITY_EXPORT_ALLOWED,
    PROP_VISUAL_APPROVED,
    REPORTS_DIR,
)
from vortex_report import Report


def main() -> int:
    report = Report("Vortex — Mark Visual Approved")
    scene = bpy.context.scene
    scene[PROP_VISUAL_APPROVED] = True
    scene[PROP_UNITY_EXPORT_ALLOWED] = True

    tagged = 0
    for obj in bpy.data.objects:
        if obj.get(PROP_IMPORT_TAG) == IMPORT_TAG_BASE or obj.name == "Vortex_Base":
            obj[PROP_APPROVED_ART] = True
            obj[PROP_IMPORT_TAG] = IMPORT_TAG_BASE
            tagged += 1

    report.add("VALGOR_VISUAL_APPROVED", True, "True")
    report.add("VALGOR_UNITY_EXPORT_ALLOWED", True, "True")
    report.add("Tagged imported base objects as approved art", tagged > 0, f"count={tagged}", level="warn")
    report.notes.append(
        "Aprovação registrada. Agora export_fbx.py / prepare_unity_import.py podem gerar handoff Unity."
    )

    REPORTS_DIR.mkdir(parents=True, exist_ok=True)
    report.write(REPORTS_DIR / "11_visual_approved.txt")
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    print(report.to_text())
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
