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
| `CitySceneHost` / `CityBootstrap` | Fundação da cidade: módulos, recursos, construções, câmera e HUD |
| `WorldMapSceneHost` / `WorldMapBootstrap` | Mapa mundial provisório |

## Cenas (Build Settings)

| Cena | Path |
|------|------|
| Bootstrap | `Assets/_Valgor/Scenes/Bootstrap.unity` |
| Loading | `Assets/_Valgor/Scenes/Loading.unity` |
| MainMenu | `Assets/_Valgor/Scenes/MainMenu.unity` |
| City | `Assets/Valgor/City/Scenes/City.unity` |
| WorldMap | `Assets/_Valgor/Scenes/WorldMap.unity` |

## Fluxo Beta Técnica 0.1

```text
Bootstrap → Loading → MainMenu → City → Heroes → Torre dos Dragões (na City) → WorldMap → City
```

Navegação provisória DDOL: `BetaNavigationBar` (Menu / Cidade / Heróis / Dragões / Mapa).
Versão: `ValgorVersion.Display` = `Beta Técnica 0.1`.
Build Windows: menu `Valgor/Build/Windows Beta Técnica 0.1` → `builds/windows/Valgor-Beta-0.1/Valgor.exe`.

## Integração de módulos

Contratos em `Valgor.Core.Modules`:

- `IPlayerCityModule`
- `IWorldMapModule`
- `IBuildingModule`
- `IResourceModule`
- `IDragonModule` / `IDragonGateway` — implementação em `Assets/Valgor/Dragons` (`DragonService`)
- `IHeroesGateway` — implementação em `Assets/Valgor/Heroes` (mesmo agente único do monorepo; D022)

## City Foundation

`Assets/Valgor/City/` contém a cidade do jogador em uma assembly própria. `CityBootstrap` registra `IPlayerCityModule` e um adaptador de `IResourceModule`, cria o catálogo provisório de edifícios e a HUD de UI Toolkit. A cidade preserva a sessão ao navegar entre `PlayerCity` e `WorldMap`.

## Dragons Foundation

`Assets/Valgor/Dragons/` — ninho, estados, alimentação, recuperação e destaque em marchas. Ver [dragons.md](dragons.md).

## Testes

Lógica pura coberta em `tools/Valgor.GameLogic.Tests` (máquina de estados, sessão, registry, SceneIds).

## Limite do assembly Game Core

O assembly Core **não** embute catálogo/facções/poderes/skins de heróis — esses vivem em `Assets/Valgor/Heroes/**` e são consumidos via `IHeroesGateway`. Um único agente pode editar ambos; não duplicar a lógica no Core.
