# Game Core Foundation

Fundação jogável do cliente Unity (Sprint Game Core).

## Fluxo inicial

```
Bootstrap
  → ServiceRegistry
  → GameSession.Begin()
  → LoadingFlow
       → Loading scene
       → MainMenu
  → GameState.MainMenu
```

## Componentes

| Tipo | Responsabilidade |
|------|------------------|
| `GameBootstrap` | Entrada DDOL, registro de serviços, dispara LoadingFlow |
| `ServiceRegistry` | Composition root tipado |
| `GameSession` | Sessão do jogador (auth token opcional) |
| `GameStateMachine` | Estados: Bootstrapping → Loading → MainMenu ↔ PlayerCity ↔ WorldMap |
| `SceneLoader` | Load/Unload assíncrono com progresso |
| `LoadingFlow` | Orquestra boot + loading screen |
| `GameNavigator` | Transições de cena/estado entre módulos |
| `ValgorGame` | Fachada estática via `GameBootstrap.Game` |

## Integração de módulos

Contratos em `Valgor.Core.Modules` (sem implementação de gameplay nesta sprint):

- `IPlayerCityModule`
- `IWorldMapModule`
- `IBuildingModule`
- `IResourceModule`
- `IDragonModule`
- `IHeroesGateway` — apenas gateway; implementação pelo agente de heróis

Módulos concretos devem se registrar no `ServiceRegistry` quando estiverem prontos. O `GameNavigator` os consome se disponíveis.

## Cenas

| Id | Status |
|----|--------|
| `Bootstrap` | Existe |
| `Loading` | Existe |
| `MainMenu` | Existe |
| `PlayerCity` | Reservada (próxima sprint) |
| `WorldMap` | Reservada (próxima sprint) |

## Limite

Não inclui catálogo, facções, poderes, magia, progressão ou skins de heróis.
