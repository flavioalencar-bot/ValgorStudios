# Player City Foundation

Cidade do jogador em `Assets/Valgor/City`.

## Fluxo

MainMenu → `GameNavigator.GoToCity()` → `CityBootstrap` → HUD / câmera / edifícios.

## Sistemas

| Área | Tipos |
|------|--------|
| Core | `CityBootstrap`, `CityController`, `BuildingSelectionService` |
| Data | `ResourceWallet`, `ResourceType`, `BuildingDefinition`, `BuildingInstance`, `BuildingState` |
| Buildings | `BuildingCatalog`, `BuildingSlot`, `BuildingView` |
| Camera | `CityCameraController`, `CityBounds` |
| UI | `CityHudController` |

## Controles

- Seleção de edifício: clique/toque esquerdo
- Pan: botão direito/meio (desktop) · arraste com um dedo (mobile)
- Zoom: scroll · pinça

## Integração

- `IPlayerCityModule` / `IResourceModule` registrados no `ServiceRegistry`
- Mapa mundial e debug menu via `GameNavigator`
- Botão Heróis apenas se `IHeroesGateway.IsAvailable`
