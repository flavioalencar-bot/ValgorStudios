# City Progression QA — evidências

## Ativação
- Manual: `Valgor.exe -cityProgressionQA` ou `scripts/run-city-progression-qa.ps1`
- Auto-test: `Valgor.exe -cityProgressionQA -cityProgressionQATest` ou `scripts/run-city-progression-qa.ps1 -AutoTest`

## Build
`builds/windows/Valgor-QA-City-Progression/Valgor.exe`

## Save
Slot `city-progression-qa` → PlayerPrefs `valgor.city.production.v1.city-progression-qa`

## Capturas
- `01-castle-nv1-tier1.png` … `08-castle-nv30-tier6.png`
- `09-qa-panel.png`
- `10-reload-nv30.png`
- `11-reset-nv1.png`
- `auto-test-report.txt`

## Trocas de tier (auto-test)
T1→T2@6, T2→T3@11, T3→T4@16, T4→T5@21, T5→T6@26 — OK
Reload Nv.30 Tier6 — OK
Reset Nv.1 — OK
