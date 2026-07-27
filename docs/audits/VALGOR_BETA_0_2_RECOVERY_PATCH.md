# VALGOR — Patch de recuperação Beta 0.2

**Data:** 2026-07-27  
**Base auditoria:** `VALGOR_BETA_0_2_*` + evidências  
**Commit alvo:** master (este patch)

## P0/P1 tratados

| ID | Correção |
|----|----------|
| B0.2-001/002/015 | `EnergyPersistenceRepository` TryParse + regravação; Seed completo; wipe limpa energia |
| B0.2-003/010 | Menu ScrollView/centragem; nav só em City/Heróis/WorldMap |
| B0.2-011 | `SaveDiagnostics` log (store Player vs Editor) |
| B0.2-012 | Missões mínimas 8 objetivos + Recolher |
| B0.2-013 | HUD mapa sem códigos técnicos |
| B0.2-014 | ResolveDisplayName + teste Lyra |
| B0.2-023 | Build sem `BuildOptions.Development` |
| NRE City | `RefreshCurrent`/`TryGetWorldAnchor` null-safe |

## Evidência pós-patch

Pasta: `docs/audits/beta-0.2-recovery-evidence/`

- Smoke exit 0  
- World Map legível (`04-worldmap.png` ~102 KB, Energia 100/100)  
- Menu sem barra inferior e sem watermark Development Build  
- `08-missions-panel.png`  
- Sem `FormatException` de energia  

## Build

Oficial pós-patch: `C:\Valgor_Studio\builds\windows\Valgor-Beta-0.2.1\Valgor.exe`  
(congeladas: `Valgor-Beta-0.1`, `Valgor-Beta-0.2`)

Revalidação: `docs/audits/VALGOR_BETA_0_2_1_REVALIDATION.md`

