"""1) Validate Vortex Blender scene structure (collections + metadata + overview)."""

from __future__ import annotations

import bpy

from vortex_common import (
    REQUIRED_COLLECTIONS,
    SCENE_METADATA_KEYS,
    CHARACTER_ID,
)
from vortex_report import Report


def _collection_names() -> set[str]:
    return {c.name for c in bpy.data.collections}


def validate_scene(report: Report | None = None) -> Report:
    report = report or Report("Vortex — Validate Scene")

    present = _collection_names()
    for name in REQUIRED_COLLECTIONS:
        report.add(
            f"Collection `{name}`",
            name in present,
            "present" if name in present else "missing",
        )

    scene = bpy.context.scene
    for key, expected in SCENE_METADATA_KEYS.items():
        if key not in scene.keys():
            report.add(f"Metadata `{key}`", False, "absent")
            continue
        value = scene[key]
        ok = value == expected or (isinstance(expected, float) and abs(float(value) - float(expected)) < 1e-6)
        report.add(
            f"Metadata `{key}`",
            ok,
            f"value={value!r} expected={expected!r}",
        )

    report.add(
        "Character id is HERO_VORTEX_000",
        scene.get("VALGOR_CHARACTER_ID") == CHARACTER_ID,
        str(scene.get("VALGOR_CHARACTER_ID")),
    )

    # Checklist text datablock from setup script
    report.add(
        "Text datablock VORTEX_EXPORT_CHECKLIST",
        "VORTEX_EXPORT_CHECKLIST" in bpy.data.texts,
        "present" if "VORTEX_EXPORT_CHECKLIST" in bpy.data.texts else "missing",
        level="warn",
    )

    return report


if __name__ == "__main__":
    from vortex_common import REPORTS_DIR

    r = validate_scene()
    out = r.write(REPORTS_DIR / "01_validate_scene.txt")
    print(r.to_text())
    print(f"Wrote {out}")
