# Valgor — Cliente Unity

Abra o Unity Hub, escolha **Add / Open** e selecione a pasta `client`. Projeto preparado para **Unity 6 LTS 6000.0.58f2**.

## Estrutura

- `Assets/_Valgor/Scenes`: `Bootstrap`, `Loading`, `MainMenu`
- `Assets/_Valgor/Scripts/Runtime/Core`: `ServiceRegistry`, `GameSession`, `GameStateMachine`, contratos de módulo
- `Assets/_Valgor/Scripts/Runtime/Bootstrap`: `GameBootstrap`
- `Assets/_Valgor/Scripts/Runtime/Scenes`: `SceneLoader`, `LoadingFlow`, loading UI
- `Assets/_Valgor/Scripts/Runtime/Navigation`: `GameNavigator`
- `Assets/_Valgor/Scripts/Runtime`: áudio, pooling, Addressables, input, localization
- `Assets/Valgor/Heroes`: catálogo, facções, poderes, magia, skins, UI roster, preview 360° (menu **Valgor → Heroes → Rebuild Catalog From Seed**)

## Fluxo

`GameBootstrap` registra serviços → `LoadingFlow` → `MainMenu`.  
Navegação posterior via `GameBootstrap.Game.Navigator`.

Documentação: `docs/architecture/game-core.md`.
