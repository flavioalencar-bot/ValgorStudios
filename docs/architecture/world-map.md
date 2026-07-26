# World Map Foundation

Mapa mundial em `Assets/Valgor/WorldMap`.

## Fluxo

City → `GameNavigator.GoToWorldMap()` → `WorldMapBootstrap` → retorno à City.

## Sistemas

| Área | Tipos |
|------|--------|
| Core | `WorldMapBootstrap`, `WorldMapController`, `RegionSelectionService` |
| Data | `WorldMapCatalog`, `RegionDefinition`, `RegionInstance` |
| Nodes | `RegionNodeView` |
| Camera | `WorldMapCameraController`, `WorldMapBounds` |
| UI | `WorldMapHudController` |

## Regiões provisórias

Floresta, Montanhas, Costa (Available) · Deserto, Ruínas, Portal (Locked).

## Controles

- Seleção: clique esquerdo no nó
- Pan: botão direito/meio
- Zoom: scroll
