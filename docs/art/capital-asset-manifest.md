# Capital — Manifesto de assets de alta fidelidade

**Pack:** `CapitalArtPack_Core_v1`  
**Catálogo:** `CityArtAssetCatalog` / `CityArtAssetCatalogLogic`

| Asset | Prioridade | Status | Fonte | LOD | Material | Integrado |
| ----- | ---------: | ------ | ----- | --- | -------- | --------- |
| architecture.castle | 1 | Ready* | CastleRealVisualLoader (Resources) | 1–6 tiers | Art Pass | Sim (loader existente) |
| architecture.dragon-tower | 1 | Missing | — | 0–3 | pedra/metal | Não (proxy) |
| architecture.wall-segment-a..d | 1 | Missing | — | 0–2 | pedra | Não |
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

\* Castelo já usa arte real por tier; slot do catálogo marca `UsesExistingCastleLoader`.  
\*\* Conteúdo HF ausente → proxy procedural homologado.

## Status permitidos

`Missing` · `Concept` · `In Production` · `Ready` · `Integrated` · `Validated`
