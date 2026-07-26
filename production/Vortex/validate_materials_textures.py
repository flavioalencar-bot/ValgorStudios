"""5) Validate provisional materials and texture files on disk."""

from __future__ import annotations

from pathlib import Path

import bpy

from vortex_common import REQUIRED_MATERIALS, REQUIRED_TEXTURES, TEXTURES_DIR, ROOT
from vortex_report import Report


def validate_materials_textures(report: Report | None = None) -> Report:
    report = report or Report("Vortex — Validate Materials & Textures")

    # Materials: placeholders must exist in blend; missing textures stay WARN until art arrives
    mats = {m.name for m in bpy.data.materials}
    for name in REQUIRED_MATERIALS:
        report.add(f"Material `{name}`", name in mats, "in blend" if name in mats else "missing in blend — run ensure_placeholder_materials.py")

    # Look for textures beside the blend / textures folder
    search_dirs = [TEXTURES_DIR, ROOT / "Textures", ROOT]
    found_any = False
    for tex_name in REQUIRED_TEXTURES:
        hits = []
        for d in search_dirs:
            if not d.exists():
                continue
            p = d / tex_name
            if p.is_file():
                hits.append(p)
        ok = len(hits) > 0
        found_any = found_any or ok
        report.add(
            f"Texture file `{tex_name}`",
            ok,
            str(hits[0]) if hits else "not on disk yet",
            level="warn",
        )

    # Images packed/loaded in blend
    image_names = {img.name for img in bpy.data.images}
    report.add(
        "Blend has painted/imported image datablocks (art)",
        any(n for n in image_names if not n.startswith("Render")),
        f"images={sorted(image_names)[:20]}",
        level="warn",
    )

    if not found_any:
        report.notes.append(
            "Nenhuma textura final encontrada. Produzir PNGs listados no brief e colocar em production/Vortex/textures/."
        )

    return report


if __name__ == "__main__":
    from vortex_common import REPORTS_DIR

    r = validate_materials_textures()
    out = r.write(REPORTS_DIR / "05_validate_materials_textures.txt")
    print(r.to_text())
    print(f"Wrote {out}")
