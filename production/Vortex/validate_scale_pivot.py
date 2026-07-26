"""2) Validate scale reference (~2.05 m) and pivot conventions."""

from __future__ import annotations

import bpy
from mathutils import Vector

from vortex_common import HEIGHT_REF_NAME, TARGET_HEIGHT_M, GROUND_NAME, TECHNICAL_OBJECT_NAMES
from vortex_report import Report


def _world_bbox(obj) -> tuple[Vector, Vector] | None:
    if obj.type != "MESH" or obj.data is None or len(obj.data.vertices) == 0:
        return None
    mats = [obj.matrix_world @ v.co for v in obj.data.vertices]
    xs = [v.x for v in mats]
    ys = [v.y for v in mats]
    zs = [v.z for v in mats]
    return Vector((min(xs), min(ys), min(zs))), Vector((max(xs), max(ys), max(zs)))


def validate_scale_pivot(report: Report | None = None) -> Report:
    report = report or Report("Vortex — Validate Scale & Pivot")

    ref = bpy.data.objects.get(HEIGHT_REF_NAME)
    report.add(f"Height reference object `{HEIGHT_REF_NAME}`", ref is not None)

    if ref is not None:
        bbox = _world_bbox(ref)
        if bbox is None:
            # Fallback: object scale/location for the setup cube
            # Setup used location z=1.025 and scale z=1.025 → half-extent 1.025 → height 2.05
            approx = abs(ref.location.z) * 2.0 if abs(ref.scale.z - ref.location.z) < 1e-3 else abs(ref.dimensions.z)
            height = approx if approx > 0 else abs(ref.dimensions.z)
        else:
            height = bbox[1].z - bbox[0].z

        ok = abs(height - TARGET_HEIGHT_M) <= 0.05
        report.add(
            "Reference height ≈ 2.05 m",
            ok,
            f"measured={height:.4f} m (tol ±0.05)",
        )

        # Pivot of reference near feet plane
        report.add(
            "Height reference base near Z=0",
            abs((bbox[0].z if bbox else (ref.location.z - abs(ref.dimensions.z) * 0.5))) <= 0.08,
            "feet/base near ground",
            level="warn",
        )

    ground = bpy.data.objects.get(GROUND_NAME)
    report.add(f"Ground reference `{GROUND_NAME}`", ground is not None, level="warn")

    # Unit scale
    unit = bpy.context.scene.unit_settings
    report.add(
        "Scene unit system METRIC / meters",
        unit.system == "METRIC" and abs(unit.scale_length - 1.0) < 1e-6,
        f"system={unit.system} scale_length={unit.scale_length}",
        level="warn",
    )

    # Real hero meshes (non-technical) — pivot check when present
    hero_meshes = [
        o for o in bpy.data.objects
        if o.type == "MESH" and o.name not in TECHNICAL_OBJECT_NAMES
    ]
    if not hero_meshes:
        report.add(
            "Final hero mesh present for pivot check",
            False,
            "no artistic mesh yet (only technical refs) — expected until art is delivered",
            level="warn",
        )
    else:
        for obj in hero_meshes:
            bbox = _world_bbox(obj)
            if bbox is None:
                continue
            min_z = bbox[0].z
            height = bbox[1].z - bbox[0].z
            report.add(
                f"Pivot/feet for `{obj.name}`",
                abs(min_z) <= 0.05,
                f"min_z={min_z:.4f} height={height:.4f}",
            )
            report.add(
                f"Height of `{obj.name}` ≈ 2.05 m",
                abs(height - TARGET_HEIGHT_M) <= 0.08,
                f"height={height:.4f}",
                level="warn",
            )

    return report


if __name__ == "__main__":
    from vortex_common import REPORTS_DIR

    r = validate_scale_pivot()
    out = r.write(REPORTS_DIR / "02_validate_scale_pivot.txt")
    print(r.to_text())
    print(f"Wrote {out}")
