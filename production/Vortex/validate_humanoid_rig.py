"""3) Validate provisional / Humanoid-ready rig."""

from __future__ import annotations

import bpy

from vortex_common import HUMANOID_BONES_MIN, RIG_OBJECT_NAME
from vortex_report import Report


def validate_humanoid_rig(report: Report | None = None) -> Report:
    report = report or Report("Vortex — Validate Humanoid Rig")

    arm_obj = bpy.data.objects.get(RIG_OBJECT_NAME)
    report.add(f"Armature object `{RIG_OBJECT_NAME}`", arm_obj is not None and arm_obj.type == "ARMATURE")

    if arm_obj is None or arm_obj.type != "ARMATURE":
        report.add("Humanoid bone set", False, "no armature")
        return report

    bones = {b.name for b in arm_obj.data.bones}
    report.add(
        "Armature has at least one bone (provisional OK)",
        len(bones) >= 1,
        f"bone_count={len(bones)} bones={sorted(bones)}",
    )

    missing = [b for b in HUMANOID_BONES_MIN if b not in bones]
    # Full Humanoid set is artistic/rigging work — warn until complete
    report.add(
        "Full Unity Humanoid bone set",
        len(missing) == 0,
        ("complete" if not missing else f"missing {len(missing)}: {', '.join(missing[:12])}"
         + ("…" if len(missing) > 12 else "")),
        level="warn" if missing else "info",
    )

    scene = bpy.context.scene
    report.add(
        "Scene metadata VALGOR_RIG_TYPE=Humanoid",
        scene.get("VALGOR_RIG_TYPE") == "Humanoid",
        str(scene.get("VALGOR_RIG_TYPE")),
    )

    # Rest pose / location
    report.add(
        "Armature at world origin (feet pivot)",
        arm_obj.location.length < 0.01,
        f"location={tuple(round(c, 4) for c in arm_obj.location)}",
        level="warn",
    )

    return report


if __name__ == "__main__":
    from vortex_common import REPORTS_DIR

    r = validate_humanoid_rig()
    out = r.write(REPORTS_DIR / "03_validate_humanoid_rig.txt")
    print(r.to_text())
    print(f"Wrote {out}")
