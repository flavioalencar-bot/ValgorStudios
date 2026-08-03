# Dragon Tower mapping correction — evidence

**Data:** 2026-08-03

## Problema
`VAL_Capital_Architecture_DragonTower` (mesh Import_v1) estava associado a `BuildingId=castle`.

## Correção
| BuildingId | Prefab / loader |
|------------|-----------------|
| `castle` | `CastleRealVisualLoader` only (HF limpo) |
| `dragon-tower` | `VAL_Capital_Architecture_DragonTower` |

Posição oficial torre: `(0, 1.35, 17.6)` — norte da Capital.

## Não alterado
BuildingId, Save, seleção, timers, DragonNestView, BuildingController.

## Screenshots
Ver `screenshots/01-castle-center.png`, `02-dragon-tower-north.png`, `03-overview-with-tower.png`.
