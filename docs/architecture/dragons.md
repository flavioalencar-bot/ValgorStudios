# Dragons Foundation

Sistema de dragões em `Assets/Valgor/Dragons`.

## Objetivo

Primeira versão funcional: ninho na Torre dos Dragões, estados, alimentação com recursos da cidade, recuperação e destaque opcional em marchas do World Map.

## Limite

Não altera o módulo interno de heróis (`Assets/Valgor/Heroes/**`). Integração apenas via `IHeroesGateway` e `IDragonGateway`.

## Entidades

| Tipo | Papel |
|------|--------|
| `DragonDefinition` / `DragonCatalog` | Catálogo estático (espécie, poder, fome) |
| `DragonInstance` | Instância do jogador |
| `DragonStateMachine` | Grafo de transições |
| `DragonRepository` | Memória (City↔WorldMap) + PlayerPrefs |
| `DragonService` | Fachada `IDragonGateway` |
| `DragonChangedEvent` | Notificação de mudança de estado |
| `DragonRoost` | Ninho vinculado à `dragon-tower` |
| `DragonFeedingService` | Alimentação (Food + DragonEssence) |
| `DragonRecoveryService` | Exhausted/Injured → Recovering → Resting; hatch |
| `DragonDeploymentService` | READY → FLYING → COMBAT / recall |

## Estados

```text
LOCKED → HATCHING → RESTING ⇄ HUNGRY ⇄ READY
READY → FLYING → COMBAT
FLYING|COMBAT → EXHAUSTED|INJURED → RECOVERING → RESTING
```

## Seed

- `dragon-ember-1` (`ember-whelp`) — READY
- `dragon-ash-1` (`ash-drake`) — LOCKED (chocável na torre)

## Integração

| Área | Comportamento |
|------|----------------|
| City | `CityBootstrap` cria/registra `DragonService`; HUD da torre lista, alimenta e choca |
| Recursos | `IDragonResourceWallet` / `CityDragonResourceWallet` (Food + Essence) |
| World Map | Despacho tenta destacar 1 dragão READY; engajar entra em COMBAT; conclusão/cancel recall + recovery |
| Poder | `GetProvisionalDragonPower()` soma poder de dragões FLYING/COMBAT ao resolver criaturas |

## Contratos Runtime

`IDragonModule`, `IDragonGateway`, `IDragonResourceWallet`, `DragonStatusInfo`, `ProvisionalDragonGateway`.
