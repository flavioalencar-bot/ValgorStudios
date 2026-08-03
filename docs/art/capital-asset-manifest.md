# Capital — Manifesto de assets de alta fidelidade

**Pack:** `CapitalArtPack_Core_v1`  
**Catálogo:** `CityArtAssetCatalog` / `CityArtAssetCatalogLogic`

| Asset | Prioridade | Status | Fonte | LOD | Material | Integrado |
| ----- | ---------: | ------ | ----- | --- | -------- | --------- |
| architecture.castle | 1 | Ready* | CastleRealVisualLoader (tiers Resources) | 1–6 | Art Pass | Loader sim; **sem** HF Import_v1 |
| architecture.dragon-tower | 1 | Integrated** | `VAL_Capital_Architecture_DragonTower` | Import mesh | URP Lit BaseColor | Sim (norte Z≈17.6) |
| architecture.wall-segment-a..d / building.wall | 1 | Integrated** | `VAL_Capital_Architecture_WallGate` | LOD0 | URP Lit BaseColor | Sim (`BuildingId=wall`) |
| architecture.gate | 1 | Missing | — | 0–2 | pedra/metal | Não |
| architecture.bridge | 1 | Missing | — | 0–2 | pedra | Não |
| architecture.fountain | 1 | Missing | — | 0–2 | pedra/água | Não |
| environment.rock-a..e | 2 | Missing | — | 0–2 | rocha | Não |
| environment.tree-a..e | 2 | Missing | — | 0–3 | casca/folhagem | Não |
| environment.bush-a..c | 2 | Missing | — | 0–2 | folhagem | Não |
| props.house-decor-a..c | 2 | Missing | — | 0–2 | madeira/telha | Não |
| materials.water | 1 | Missing | — | Medium/High | água PBR | Não |
| environment.mountain-distant | 1 | Missing | — | Low | rocha/névoa | Não |
| building.* (17 oficiais) | 1 | Missing** | slots prontos | 0–2 | por distrito | Fallback |

\* Castelo: tiers Resources + `UsesExistingCastleLoader` (sem HF Import_v1 — correção 2026-08-03).  
\*\* Mesh Import_v1 remapeado para `dragon-tower` → `VAL_Capital_Architecture_DragonTower`. Nunca Castle→DragonTower.  
\*\*\* Conteúdo HF ausente nos demais → proxy procedural homologado.

## Status permitidos

`Missing` · `Concept` · `In Production` · `Ready` · `Integrated` · `Validated`
