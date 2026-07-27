# Validação Beta 0.1 — 2026-07-27

## Build

| Campo | Resultado |
|-------|-----------|
| Script | `scripts/build-windows-beta.ps1` → checkpoint |
| Log | `builds/windows/checkpoint-build.log` → **Build Successful** / `[Valgor] Build OK` |
| EXE | `builds/windows/Valgor-Checkpoint\Valgor.exe` (espelho: `Valgor-Beta-0.1\`) |
| Smoke | `-checkpointSmoke -captureEvidence` → **exit 0** |

## Fluxo validado

| Etapa | Resultado |
|-------|-----------|
| Main Menu (Beta 0.1) | OK |
| City | OK — `[CheckpointSmoke] City OK` |
| Heróis (Vortex) | OK — `[CheckpointSmoke] HeroesDemo OK` |
| Dragões | OK — foca **Torre dos Dragões** na City (sem cena dedicada) |
| World Map + nó | OK — `[CheckpointSmoke] WorldMap OK` |
| Retorno City | OK — jornada mínima concluída |

## Capturas

Pasta: `docs/releases/beta-0.1-evidence/`

- `00-main-menu.png`
- `01-city.png` … `05-worldmap-filters-open.png` (smoke)
- `08-dragons.png` (Torre dos Dragões selecionada)

## Erros reais observados (não bloqueantes)

1. `The referenced script on this Behaviour (Game Object '<null>') is missing!` (2× no boot)
2. `[Valgor.Localization] Catálogo Addressables ausente` — fallback para strings embutidas
3. `XInput1_3.dll not found` — fallback para `XInput9_1_0.dll`

Sem `NullReferenceException`, sem crash no smoke (exit **0**).
