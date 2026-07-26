"""4) Validate required Vortex sockets."""

from __future__ import annotations

import bpy

from vortex_common import REQUIRED_SOCKETS
from vortex_report import Report


def validate_sockets(report: Report | None = None) -> Report:
    report = report or Report("Vortex — Validate Sockets")

    objects = {o.name: o for o in bpy.data.objects}
    sock_col = bpy.data.collections.get("VORTEX_SOCKETS")

    for name in REQUIRED_SOCKETS:
        obj = objects.get(name)
        ok = obj is not None
        detail = ""
        if obj is not None:
            detail = f"type={obj.type} loc=({obj.location.x:.3f}, {obj.location.y:.3f}, {obj.location.z:.3f})"
            if sock_col is not None and obj.name not in sock_col.objects:
                # Still OK if linked elsewhere, but warn
                report.add(
                    f"Socket `{name}` in VORTEX_SOCKETS collection",
                    False,
                    "object exists but not in VORTEX_SOCKETS",
                    level="warn",
                )
        report.add(f"Socket `{name}`", ok, detail or "missing")

    report.add(
        "Collection VORTEX_SOCKETS exists",
        sock_col is not None,
    )

    return report


if __name__ == "__main__":
    from vortex_common import REPORTS_DIR

    r = validate_sockets()
    out = r.write(REPORTS_DIR / "04_validate_sockets.txt")
    print(r.to_text())
    print(f"Wrote {out}")
