# Player City Foundation

Cidade do jogador em `Assets/Valgor/City`.

## Fluxo

MainMenu → `GameNavigator.GoToCity()` → `CityBootstrap` → HUD / câmera / edifícios / produção.

## Sistemas

| Área | Tipos |
|------|--------|
| Core | `CityBootstrap`, `CityController`, `CityEconomy`, `BuildingSelectionService` |
| Data | `ResourceWallet`, `BuildingDefinition`, `ProductionCatalog`, `IGameClock` |
| Production | `ResourceProductionService`, `OfflineProductionCalculator`, `ResourceCollectionService`, `ProductionTickService`, `LocalProductionRepository` |
| Buildings | `BuildingCatalog`, `BuildingSlot`, `BuildingView` |
| Camera | `CityCameraController`, `CityBounds` |
| UI | `CityHudController` (inclui painel da Torre dos Dragões) |

## Torre dos Dragões

Produz `DragonEssence`. Selecionada, a HUD lista o ninho (`DragonService` / `IDragonGateway`), permite alimentar e chocar ovos. Detalhes: [dragons.md](dragons.md).

## Produção passiva

Produtores: Fazenda (Food), Serraria (Wood), Pedreira (Stone), Mina (Iron), Mercado (Gold), Torre dos Dragões (DragonEssence).

- Taxa e capacidade por nível via `ProductionCatalog` (configurável)
- Acúmulo com capacidade máxima; coleta manual
- Offline até 12h; tick por timestamp (não por FPS)
- Persistência local + `CityEconomy` no `ServiceRegistry` (City ↔ WorldMap)
- Diamonds sem produção passiva

## Controles

- Seleção: clique/toque esquerdo
- Pan: botão direito/meio · arraste
- Zoom: scroll · pinça
