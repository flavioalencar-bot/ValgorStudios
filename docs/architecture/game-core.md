# Game Core Foundation

Fundação jogável do cliente Unity.

## Fluxo validado

```
GameBootstrap
  → ServiceRegistry + GameSession
  → LoadingFlow
  → MainMenu
  → City
  → WorldMap
  → City (sessão preservada via DDOL)
```

## Componentes

| Tipo | Responsabilidade |
|------|------------------|
| `GameBootstrap` | Entrada DDOL, registry, LoadingFlow |
| `ServiceRegistry` | Composition root tipado |
| `GameSession` | Sessão do jogador (preservada entre cenas) |
| `GameStateMachine` | Bootstrapping → Loading → MainMenu ↔ PlayerCity ↔ WorldMap |
| `SceneLoader` | Load/Unload assíncrono com progresso |
| `LoadingFlow` | Orquestra boot + loading screen |
| `GameNavigator` | `GoToCity`, `GoToWorldMap`, `GoToMainMenu` |
| `MainMenuSceneHost` | UI do menu + entrada na cidade |
| `CitySceneHost` / `ProvisionalCityBootstrap` | Cidade provisória (navegação) |
| `WorldMapSceneHost` / `WorldMapBootstrap` | Mapa mundial provisório |

## Cenas (Build Settings)

| Cena | Path |
|------|------|
| Bootstrap | `Assets/_Valgor/Scenes/Bootstrap.unity` |
| Loading | `Assets/_Valgor/Scenes/Loading.unity` |
| MainMenu | `Assets/_Valgor/Scenes/MainMenu.unity` |
| City | `Assets/Valgor/City/Scenes/City.unity` |
| WorldMap | `Assets/_Valgor/Scenes/WorldMap.unity` |

## Integração de módulos

Contratos em `Valgor.Core.Modules`:

- `IPlayerCityModule`
- `IWorldMapModule`
- `IBuildingModule`
- `IResourceModule`
- `IDragonModule`
- `IHeroesGateway` — implementação pelo agente de heróis

## Testes

Lógica pura coberta em `tools/Valgor.GameLogic.Tests` (máquina de estados, sessão, registry, SceneIds).

## Limite

Não inclui catálogo/facções/poderes/skins de heróis.
