# Homologação Dragão — Fases 1 a 4

**Base:** `fbee9bc`  
**Build homologação:** `Valgor_Studio_phase4_homolog/builds/windows/Valgor-QA-Dragon-Phase4-Homolog/Valgor.exe`  
**Persistência:** `valgor.dragons.v7`  
**CLI:** `-dragonPhases14Homolog`  
**Fase 5:** não iniciada nesta homologação

## Resultado final

| Critério | Status |
|----------|--------|
| E2E Fases 1–4 | PASS |
| P0 | 0 |
| P1 | 0 |
| DragonFoundation | 33/0 |
| Persistência v7 | OK |

Veredito: `FINAL_VERDICT.md` · Relatório: `homolog-report.txt`

## Prep

- Backup REG: `backups/playerprefs-full-20260730-191515.reg`
- Perfil QA limpo antes do E2E
- Hard reset no harness (Castelo 1 / ovo LOCKED) para não contaminar T1

## Matriz E2E

| Item | Resultado | Evidência |
|------|----------|-----------|
| T1 Castelo 19 bloqueado | PASS | `screenshots/t01-castle19-tower-locked.png` |
| T2 Castelo 20 unlock | PASS | `screenshots/t02-castle20-unlocked.png` |
| T3 Nascimento Nv.1 | PASS | `screenshots/t03-dragon-nv1.png` |
| Rituais 6/11/16/21/26 | PASS | `screenshots/ritual-*` / `after-nv*` |
| PvE avançado bloqueado Nv.15 | PASS | `t04-pve-advanced-blocked-nv15.png` |
| PvE liberado Nv.16 | PASS | `t05-pve-unlocked-nv16.png` |
| Montaria ritual bloqueada Nv.20 | PASS | `t06-mount-ritual-blocked-nv20.png` |
| Montaria liberada Nv.21 | PASS | `t07-mount-unlocked-nv21.png` |
| Habilidades + combate + recall | PASS | `t08-pve-injured-recall.png` |
| Vortex + marcha + retorno | PASS | `t09` / `t10` |
| Nv.30 Ancestral | PASS | `t11-nv30-ancestral.png` |
| Save/reload offline | PASS | `t12-save-reload.png` |
| 1080×640 | PASS | `t13-responsive-1080x640.png` |

## Como reproduzir

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-qa-dragon-phase4-homolog.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/run-dragon-phases14-homolog.ps1
```
