"""8) Prepare files for Unity import (staging only — does not invent art)."""

from __future__ import annotations

import json
import os
import shutil
import sys
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import bpy

from vortex_common import (
    EXPORT_DIR,
    STAGING_DIR,
    TEXTURES_DIR,
    UNITY_MODELS_REL,
    UNITY_TEXTURES_REL,
    CHARACTER_ID,
    ROOT as VORTEX_ROOT,
    PROP_VISUAL_APPROVED,
    PROP_UNITY_EXPORT_ALLOWED,
)
from vortex_report import Report


def prepare_unity_import(report: Report | None = None) -> Report:
    """
    Copies validated FBX + textures into production/Vortex/unity_staging/
    and writes a manifest with target Unity paths.

    Blocked until VALGOR_VISUAL_APPROVED and VALGOR_UNITY_EXPORT_ALLOWED.
    Never copies into Unity unless COPY_INTO_UNITY=1 after approval.
    """
    report = report or Report("Vortex — Prepare Unity Import")
    STAGING_DIR.mkdir(parents=True, exist_ok=True)
    models_stage = STAGING_DIR / "Models"
    textures_stage = STAGING_DIR / "Textures"
    models_stage.mkdir(exist_ok=True)
    textures_stage.mkdir(exist_ok=True)

    scene = bpy.context.scene
    visual_ok = bool(scene.get(PROP_VISUAL_APPROVED, False))
    export_ok = bool(scene.get(PROP_UNITY_EXPORT_ALLOWED, False))
    report.add(
        "Visual approval required before Unity handoff",
        visual_ok and export_ok,
        f"{PROP_VISUAL_APPROVED}={visual_ok} {PROP_UNITY_EXPORT_ALLOWED}={export_ok}",
        level="warn" if not (visual_ok and export_ok) else "info",
    )

    if not (visual_ok and export_ok):
        manifest = {
            "character_id": CHARACTER_ID,
            "generated_utc": datetime.now(timezone.utc).isoformat(),
            "blocked": True,
            "reason": "VALGOR_VISUAL_APPROVED / VALGOR_UNITY_EXPORT_ALLOWED are false",
            "unity_target_models": str(Path(r"C:\Valgor_Studio") / UNITY_MODELS_REL),
            "unity_target_textures": str(Path(r"C:\Valgor_Studio") / UNITY_TEXTURES_REL),
        }
        path = STAGING_DIR / "unity_import_manifest.json"
        path.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
        report.add("Wrote blocked unity_import_manifest.json", path.is_file(), str(path))
        report.notes.append(
            "Unity staging/copy blocked until visual approval. Base import is allowed; Unity export is not."
        )
        return report

    fbx_files = sorted(EXPORT_DIR.glob("Vortex_*.fbx")) if EXPORT_DIR.exists() else []
    report.add(
        "FBX ready in export/",
        len(fbx_files) > 0,
        (
            ", ".join(p.name for p in fbx_files)
            if fbx_files
            else "none — run export_fbx.py after artistic mesh exists"
        ),
        level="warn" if not fbx_files else "info",
    )

    staged = []
    for fbx in fbx_files:
        dest = models_stage / fbx.name
        shutil.copy2(fbx, dest)
        staged.append(dest)
        report.add(f"Staged model `{fbx.name}`", dest.is_file(), str(dest))

    tex_copied = 0
    for d in (TEXTURES_DIR, ROOT / "Textures"):
        if not d.is_dir():
            continue
        for png in d.glob("Vortex_*.png"):
            shutil.copy2(png, textures_stage / png.name)
            tex_copied += 1
    report.add("Staged textures", tex_copied > 0, f"copied={tex_copied}", level="warn")

    manifest = {
        "character_id": CHARACTER_ID,
        "generated_utc": datetime.now(timezone.utc).isoformat(),
        "blocked": False,
        "staging_models": [str(p) for p in staged],
        "unity_target_models": str(Path(r"C:\Valgor_Studio") / UNITY_MODELS_REL),
        "unity_target_textures": str(Path(r"C:\Valgor_Studio") / UNITY_TEXTURES_REL),
        "next_unity_steps": [
            "Place Vortex_LOD0.fbx (and LOD1/LOD2) into Assets/Valgor/Heroes/Characters/Vortex/Models/",
            "Place textures into .../Textures/",
            "In Unity: Valgor → Heroes → Vortex → Validate Source Assets",
            "Valgor → Heroes → Vortex → Build Vortex Prefab",
            "Confirm PrefabAddress heroes/HERO_VORTEX_000/prefab",
        ],
    }
    manifest_path = STAGING_DIR / "unity_import_manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    report.add("Wrote unity_import_manifest.json", manifest_path.is_file(), str(manifest_path))

    readme = STAGING_DIR / "HOW_TO_IMPORT_UNITY.md"
    readme.write_text(
        "\n".join(
            [
                "# Unity import — Vortex",
                "",
                f"Character: `{CHARACTER_ID}`",
                "",
                "## Staging",
                f"- Models: `{models_stage}`",
                f"- Textures: `{textures_stage}`",
                "",
                "## Target in repo",
                f"- `{UNITY_MODELS_REL}`",
                f"- `{UNITY_TEXTURES_REL}`",
                "",
                "## Rules",
                "- Only after visual approval.",
                "- Never promote Vortex_HeightReference / GroundReference as the hero.",
                "",
            ]
        ),
        encoding="utf-8",
    )
    report.add("Wrote HOW_TO_IMPORT_UNITY.md", readme.is_file())

    copy_into_unity = os.environ.get("COPY_INTO_UNITY", "0") == "1"
    if copy_into_unity and fbx_files:
        unity_models = Path(r"C:\Valgor_Studio") / UNITY_MODELS_REL
        unity_models.mkdir(parents=True, exist_ok=True)
        for fbx in fbx_files:
            shutil.copy2(fbx, unity_models / fbx.name)
            report.add(f"Copied into Unity Models `{fbx.name}`", True, str(unity_models / fbx.name))
    else:
        report.notes.append(
            "Staging only (default). To copy into Unity Models set COPY_INTO_UNITY=1 after approval."
        )

    return report


if __name__ == "__main__":
    from vortex_common import REPORTS_DIR

    r = prepare_unity_import()
    out = r.write(REPORTS_DIR / "08_prepare_unity_import.txt")
    print(r.to_text())
    print(f"Wrote {out}")
