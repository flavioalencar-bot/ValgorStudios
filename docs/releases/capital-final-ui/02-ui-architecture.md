# 02 — Arquitetura UI Capital Final

## Stack

- UIToolkit (`UIDocument`) — sem uGUI Canvas no fluxo Capital
- Tema: `ValgorUiTheme` + `BetaVisualTheme` (cores base)
- Layout: `ValgorHudLayoutConfig` (ref 1920×1080)
- Ícones lógicos: `ValgorIconCatalog`
- Tipografia: `ValgorTypographyConfig`
- Materiais UI: `ValgorUiMaterialCatalog`

## Componentes

| Zona | Tipo | Classe |
|------|------|--------|
| Jogador | Presenter/View/VM | `PlayerHudPresenter`, `PlayerHudView`, `PlayerHudViewModel` |
| Recursos | chips | `CityHudController` |
| Filas | painel | `CityHudController` |
| Menus topo | shell disabled | `CapitalFinalHudConfig.DisabledTopMenus` |
| Atalhos | botões | Construção/Pesquisa/Missões/Explorar |
| Chat | provider | `IChatProvider` / `LocalSystemChatProvider` |
| Inferior | nav | `BetaNavigationBar` |
| Labels | mundo | `CityBuildingWorldLabel` + `CityBuildingLabelManager` |

## Separação

- View não acessa PlayerPrefs/save
- Presenter monta ViewModel a partir de `LocalPlayerProfile` + `CityController`
- Chat remoto: stub `RemoteChatProvider` (não usado)
