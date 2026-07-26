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
| `DragonStateMachine` | Grafo de transições oficiais |
| `DragonRepository` | Memória (City↔WorldMap) + PlayerPrefs (`valgor.dragons.v2`) |
| `DragonService` | Fachada `IDragonGateway` |
| `DragonChangedEvent` | Notificação de mudança de estado |
| `DragonRoost` | Ninho vinculado à `dragon-tower` |
| `DragonFeedingService` | Alimentação (Food + DragonEssence) |
| `DragonHungerService` | Decaimento de fome → HUNGRY |
| `DragonRecoveryService` | Hatch, juvenile, recovering, resting |
| `DragonDeploymentService` | READY → DEPLOYED / recall |

## Estados oficiais

```text
LOCKED → EGG → HATCHING → JUVENILE → RESTING ⇄ READY
READY → DEPLOYED → EXHAUSTED|INJURED → RECOVERING → RESTING
READY|RESTING|JUVENILE → HUNGRY → RESTING|READY
```

Timers (configuráveis em `DragonSettings`): hatch, juvenile, rest, recovery, intervalo de fome.

## Seed

- `dragon-ember-1` (`ember-whelp`) — READY
- `dragon-ash-1` (`ash-drake`) — LOCKED (desbloqueia ovo e choca na torre)

## Integração

| Área | Comportamento |
|------|----------------|
| City | `CityBootstrap` cria/registra `DragonService`; HUD da torre lista, alimenta e choca |
| Recursos | `IDragonResourceWallet` / `CityDragonResourceWallet` (Food + Essence) |
| World Map | Despacho destaca 1 dragão READY (DEPLOYED); combate permanece DEPLOYED; conclusão/cancel recall + recovery |
| Poder | `GetProvisionalDragonPower()` soma poder de dragões DEPLOYED |

## Contratos Runtime

`IDragonModule`, `IDragonGateway`, `IDragonResourceWallet`, `DragonStatusInfo`, `ProvisionalDragonGateway`.
