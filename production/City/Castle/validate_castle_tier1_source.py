#!/usr/bin/env python3
"""Valida presença e metadados básicos do Castelo Tier 1 (source).

Não importa no Unity. Não altera a City.
Enquanto o GLB/FBX não existir: BLOQUEADO POR ASSET REAL.
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
CASTLE = Path(__file__).resolve().parent
SOURCE = CASTLE / "source"
MANIFEST = CASTLE / "unity_staging" / "unity_import_manifest.json"
CANDIDATES = ("Castle_Tier1.glb", "Castle_Tier1.fbx")

# Convenção de escala (metros Unity), alinhada ao manifesto.
FOOTPRINT_MIN = 5.5
FOOTPRINT_MAX = 9.0
HEIGHT_MAX = 12.0


def main() -> int:
    present = [name for name in CANDIDATES if (SOURCE / name).is_file()]
    blocked = len(present) == 0

    report = {
        "status": "BLOQUEADO POR ASSET REAL" if blocked else "ASSET PRESENTE — aguardando importação sob ordem",
        "blocked": blocked,
        "source_dir": str(SOURCE),
        "present": present,
        "missing": [n for n in CANDIDATES if n not in present],
        "scale_rules": {
            "footprint_xz": [FOOTPRINT_MIN, FOOTPRINT_MAX],
            "height_max": HEIGHT_MAX,
            "pivot": "base center Y=0",
            "forward": "+Z main gate",
        },
        "note": "Validação de bounds reais do mesh exige Blender/Unity na etapa de importação.",
    }

    if MANIFEST.is_file():
        data = json.loads(MANIFEST.read_text(encoding="utf-8"))
        data["blocked"] = blocked
        data["source_present"] = [str(SOURCE / n) for n in present]
        data["status"] = report["status"]
        if not blocked:
            data["blocked_reason"] = ""
        else:
            data["blocked_reason"] = (
                "BLOQUEADO POR ASSET REAL — aguardando Castle_Tier1.glb ou "
                "Castle_Tier1.fbx em production/City/Castle/source/"
            )
        MANIFEST.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        report["manifest_updated"] = str(MANIFEST)

    print(json.dumps(report, indent=2, ensure_ascii=False))
    return 1 if blocked else 0


if __name__ == "__main__":
    sys.exit(main())
