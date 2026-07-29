# Dragão Fase 2 P1 — Visuais por estágio + E2E Nv.1→30

**Data:** 2026-07-29  
**Base:** Fase 2 `32f7454` / docs `cef7078`  
**Persistência:** `valgor.dragons.v5` (E2E usa `valgor.dragons.v5.phase2-e2e`)  
**Build:** `builds/windows/Valgor-QA-Dragon-Phase2/Valgor.exe`

## Escopo P1

| Item | Status |
|------|--------|
| Catálogo `DragonStageVisualConfig` data-driven | OK |
| Placeholders 3D próprios diferenciados (7 estágios) | OK |
| Troca só na conclusão do ritual (sem antecipar) | OK |
| Timer + VFX leve durante ritual | OK |
| Câmera/root estáveis na troca | OK |
| Harness E2E Unity Nv.1→30 | OK (`-dragonPhase2E2E`) |

## Estágios visuais

| Estágio | Níveis |
|---------|--------|
| Ovo | pré-nascimento |
| Filhote | 1–5 |
| Jovem | 6–10 |
| Adolescente | 11–15 |
| Adulto jovem | 16–20 |
| Adulto | 21–25 |
| Ancestral | 26–30 |

Substituição futura: asset `Resources/Valgor/Dragons/DragonStageVisualCatalog` + prefabs nos paths do config (`PlaceholderFlag=false`).

## Como rodar E2E

```powershell
.\scripts\run-dragon-phase2-e2e.ps1
```

Flags: `-cityProgressionQA -dragonPhase2E2E`  
Evidências: esta pasta (`e2e-report.txt` + PNGs).

## Testes

```text
dotnet test tools/Valgor.GameLogic.Tests --filter FullyQualifiedName~DragonFoundation
→ 27 pass / 0 fail

.\scripts\run-dragon-phase2-e2e.ps1
→ PASS Nv.30 Ancestral
```

## Fora (Fase 3)

Combate, PvP, montaria, múltiplos dragões.
