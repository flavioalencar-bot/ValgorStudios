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
| `DragonGrowthService` / `DragonBondService` / `DragonEvolutionService` | Crescimento, vínculo e evolução |
| `DragonDeploymentService` | READY → DEPLOYED / recall |

## Estados oficiais

```text
LOCKED → EGG → HATCHING → JUVENILE → RESTING ⇄ READY
READY → DEPLOYED → EXHAUSTED|INJURED → RECOVERING → RESTING
READY|RESTING|JUVENILE → HUNGRY → RESTING|READY
```

Timers (configuráveis em `DragonSettings`): hatch, juvenile, rest, recovery, intervalo de fome.

## Crescimento, evolução e vínculo

Eixo separado do estado operacional:

```text
EGG → HATCHLING → JUVENILE → ADULT → ELDER → ANCIENT
```

| Tipo | Papel |
|------|--------|
| `DragonGrowthService` | Sync com ciclo de vida + pontos → avanço de estágio |
| `DragonBondService` | Pontos/nível de vínculo (alimentação e missões) |
| `DragonEvolutionService` | Cadeia `ember-whelp → ash-drake → portal-wyrm` |

Poder provisório = `BasePower × multiplicador de crescimento × (1 + 0.05 × BondLevel)`.
Persistência: `valgor.dragons.v3`.

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
