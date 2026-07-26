# World Map

Mapa mundial em `Assets/Valgor/WorldMap`.

## Fluxo

City → `GameNavigator.GoToWorldMap()` → `WorldMapBootstrap` → retorno à City.

Estado (`WorldMapSession`, marchas, nós) sobrevive City↔WorldMap via `ServiceRegistry`.

## Sistemas

| Área | Tipos |
|------|--------|
| Core | `WorldMapBootstrap`, `WorldMapController`, `WorldMapSession`, `MarchService`, `TravelTimeCalculator`, `WorldResourceHarvestService` |
| Data | Regiões, `WorldNodeCatalog`, tipos `WorldCityNode`…`WorldLandmarkNode`, `WorldMapSettings` |
| Nodes | `RegionNodeView`, `WorldNodeView` |
| Camera | `WorldMapCameraController`, `WorldMapBounds` |
| UI | `WorldMapHudController` (inspeção, marcha, coleta, HUD de recursos) |

## Tipos de nó

- `WorldCityNode` — cidades (inclui base do jogador)
- `WorldVillageNode` — vilarejos
- `WorldResourceNode` — pontos de coleta
- `WorldCreatureNode` — criaturas (inspeção; combate em sprint futura)
- `WorldDragonNode` — dragões (inspeção)
- `WorldLandmarkNode` — marcos

## Marcha provisória

1. Selecionar nó disponível  
2. Estimar tempo (`distância / MarchSpeedUnitsPerHour`)  
3. `IHeroesGateway.TryReserveMarchSlot` (stub `ProvisionalHeroesGateway` até o módulo de heróis)  
4. Avanço por timestamp (`MarchService.Advance`) — independente de FPS  
5. No destino: coletar (recursos) e/ou retornar  

## Controles

- Seleção: clique esquerdo no nó  
- Pan: botão direito/meio  
- Zoom: scroll  
