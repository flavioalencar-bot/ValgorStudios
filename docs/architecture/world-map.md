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
