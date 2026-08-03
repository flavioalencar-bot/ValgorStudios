# Capital — Muralha HF (WallGate)

**Data:** 2026-08-03  
**Pack:** `VAL_Capital_Architecture_WallGate`  
**BuildingId:** `wall` (único — sem Gate/Wall/Tower Building separado)

## Escopo
Integração **apenas visual** via `CityArtAssetCatalog` + `CityArtReplacementController`.  
Estrutura: `Visual / WallVisualRoot / ArtHF`.

## Não alterado
Save · Upgrade · Reparo · Durabilidade · Seleção · Fases 1–5 · Castle · DragonTower · BuildingId

## Fallback
Sem prefab / validação falha → muralha procedural (`BuildWallGatehouse`) permanece.

## LOD
LOD0 = prefab atual. Pastas LOD1–3 preparadas; sem auto-simplificação.

## Screenshots
Ver `screenshots/01-overview` … `09-overview1080x640`.

## Build
`builds/windows/Valgor-QA-Capital-WallHF/Valgor.exe`
