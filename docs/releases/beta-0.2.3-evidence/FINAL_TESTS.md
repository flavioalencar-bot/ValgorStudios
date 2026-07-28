# Beta 0.2.3 — testes finais (pré-commit)

Data: 2026-07-28

## Build

- Exe: `builds/windows/Valgor-Beta-0.2.3/Valgor.exe` (MZ, 672256 bytes, 2026-07-28 18:27:32)
- Log: `Build Successful` + `[Valgor] Build OK`
- Prefabs: `Castle tiers prefabs OK` (Tier1–6 Resources)
- Builds 0.1 / 0.2 / 0.2.1 / 0.2.2 preservadas

## Assets (1–6)

Cada tier: GLB source + FBX Unity + BaseColor + URP/Lit mat + Prefab Visual + Resources `Valgor/Castle_TierN` — OK

## Lógica

- Faixas de nível 1–5…26–30 → Tier1…6 verificadas
- `CastleRealVisualLoader`, `CastleTierTransition`, `BuildingView.SyncCastleVisual`, `CityController.SyncCastleVisuals` presentes
- Sem recolor genérico nos assets reais

## Evidência versionada

- `docs/releases/beta-0.2.3-evidence/castle-tiers-validation.md`
- `production/City/Castle/reports/prepare_all_tiers.json`

## Smoke checkpoint

- Comando: `scripts/capture-checkpoint-evidence.ps1 -Exe ...\Valgor-Beta-0.2.3\Valgor.exe`
- Exit: 0
- PNGs: 98
- Amostra: `docs/releases/beta-0.2.3-evidence/smoke/`
