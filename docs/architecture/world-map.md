# World Map

Mapa mundial em `Assets/Valgor/WorldMap`.

## Fluxo

City → `GameNavigator.GoToWorldMap()` → `WorldMapBootstrap` → retorno à City.

Estado (`WorldMapSession`, marchas, nós, energia) sobrevive City↔WorldMap via `ServiceRegistry`.

## Sistemas

| Área | Tipos |
|------|--------|
| Core | `WorldMapBootstrap`, `WorldMapController`, `WorldMapSession`, `MarchService`, `TravelTimeCalculator`, `WorldResourceHarvestService` |
| Energy | `PlayerEnergyWallet`, `EnergyRegenerationService`, `EnergyCostResolver`, `EnergyChangedEvent`, `EnergyPersistenceRepository` |
| Filters / Locate / Territory | `WorldMapFilterService`, `WorldMapLocatorService`, `WorldTerritoryDefinition`, overlays |
| Data | Regiões, `WorldNodeCatalog`, tipos `WorldCityNode`…`WorldLandmarkNode`, `WorldMapSettings` |
| Nodes | `RegionNodeView`, `WorldNodeView` |
| Camera | `WorldMapCameraController`, `WorldMapBounds` |
| UI | `WorldMapHudController` (inspeção, marcha, coleta, energia/regen, HUD de recursos) |

## Energia

Campos: `currentEnergy`, `maxEnergy`, `lastUpdatedAt`, `regenIntervalSec`, `regenAmount`.

| Tipo | Papel |
|------|--------|
| `PlayerEnergyWallet` | Saldo, spend/add, clamp e `EnergyChangedEvent` |
| `EnergyRegenerationService` | Regen determinístico por timestamp (anti-duplicação na reconexão) |
| `EnergyCostResolver` | Custos configuráveis (`DispatchMarch`, `EngageCreature`) |
| `EnergyPersistenceRepository` | Memória (City↔WorldMap) + PlayerPrefs (restart); contrato pronto para backend |

HUD exibe `current/max` e ETA até energia cheia. Engajar criatura e (se configurado) despachar marcha consomem energia.

## Filtros, localizar e visão territorial

| Tipo | Papel |
|------|--------|
| `WorldMapFilterState` / `WorldMapFilterService` | Seleção combinável (tipos + ocupados/disponíveis) |
| `WorldNodeVisibilityResolver` | Visibilidade sem destruir estado dos nós |
| `WorldMapFilterPanel` | Painel UI Toolkit + limpar filtros |
| `WorldMapLocatorService` | Alvos: casa, marcha, seleção, criatura, recurso |
| `WorldCameraFocusRequest` | Pedido de foco com zoom configurável |
| `WorldMapBounds.ClampPosition` | Impede ultrapassar limites do mapa |
| `WorldTerritoryDefinition` / `WorldTerritoryState` | Fundação de territórios por região |
| `WorldTerritoryOverlay` / `WorldTerritoryColorResolver` | Overlay visual (Neutral/Owned/Allied/Enemy/Contested/Locked) |

Filtros persistem via `WorldMapFilterPersistenceRepository` (memória + PlayerPrefs) e sobrevivem City↔WorldMap.

## Restauração (patch final)

| Gap | Correção |
|-----|----------|
| Câmera/zoom | `WorldCameraState` + `WorldCameraPersistenceService` (default só se não houver estado) |
| Seleção | `selectedNodeId` no snapshot; `RestoreFromId` após reload |
| Tick fora do mapa | `GlobalMarchTickService` + `WorldSimulationCoordinator` + host DDOL |
| Carteira no depósito | `rewardDeliveryId` / `IsCommitted` + `PersistWallet` atômico |

## Tipos de nó

- `WorldCityNode` — cidades (inclui base do jogador)
- `WorldVillageNode` — vilarejos
- `WorldResourceNode` — pontos de coleta
- `WorldCreatureNode` — criaturas (inspeção; combate em sprint futura)
- `WorldDragonNode` — dragões (inspeção)
- `WorldLandmarkNode` — marcos

## Coleta, carga e respawn

Nós de recurso (`WorldResourceNode`) possuem: `resourceType`, `maxAmount`, `level`, `gatherRatePerHour`, `respawnDuration`.

Runtime (`WorldNodeInstance`): `remainingAmount`, `respawnAt`, `occupiedByMarchId`, `resourceState`.

Estados do recurso: `Available` → `Occupied` → `Depleted`/`Respawning` → `Available`.

| Tipo | Papel |
|------|--------|
| `ResourceGatherCalculator` | Cálculo determinístico por timestamp |
| `WorldResourceGatheringService` | Inicia coleta, aplica taxa, respawn, depósito da carga |

Fluxo: chegar → Coletar (GATHERING) → carga sobe na marcha → retornar → ao completar, deposita na carteira uma vez.

## Marcha completa e ocupação


Estados: `Preparing` → `Marching` → `Arrived` → `Gathering` → `Returning` → `Completed` (ou `Cancelled`).

| Tipo | Papel |
|------|--------|
| `MarchStateMachine` | Transições válidas/inválidas |
| `MarchService` | Despacho, avanço por timestamp, cancelamento, carga |
| `MarchRepository` | Persistência da marcha ativa |
| `MarchTravelCalculator` | Tempo de deslocamento |
| `WorldNodeOccupationService` | `occupiedByMarchId` exclusivo |
| `MarchChangedEvent` | Notificação de mudança |

Regras: um nó de recurso ocupado rejeita outra marcha; liberação no retorno/cancelamento/conclusão; recompensa entregue uma única vez (`RewardsDelivered`).

## Criaturas

| Tipo | Papel |
|------|--------|
| `WorldCreatureDefinition` / `WorldCreatureInstance` | Dados e estado runtime |
| `CreatureRewardTable` | Recompensas configuráveis |
| `CreatureDifficultyResolver` | Faixa vs poder provisório |
| `CreatureEncounterService` | Engajar → resolver → respawn |

Estados: `Available` → `Engaged` → `Defeated` → `Respawning` → `Available`.

## Controles

- Seleção: clique esquerdo no nó  
- Pan: botão direito/meio  
- Zoom: scroll  
