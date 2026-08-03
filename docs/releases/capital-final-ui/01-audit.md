# 01 — Auditoria Capital Final UI

**Data:** 2026-08-02  
**Stack:** UIToolkit (sem uGUI Canvas no fluxo Capital)  
**Labels mundo:** TextMesh 3D em `BuildingView`

## Tabela referência → estado

| Elemento da referência | Status | Fonte dos dados | Arquivo responsável | Risco |
|---|---|---|---|---|
| Painel jogador (retrato/nome/poder/VIP/nível/XP) | parcial → a implementar | `LocalPlayerProfile` (nome); nível = Castelo real; poder/VIP **ausentes** | `CityHudController` → `PlayerHudPresenter` | Não inventar VIP/poder |
| Barra de recursos com ícones | parcial | `CityEconomy.Wallet` | `CityHudController` (label monolítico) | Manter precisão persistida |
| Menus topo direita (Passe/Eventos/Loja) | ausente | — | shell disabled | Sem mecânica falsa |
| Filas laterais (construção/treino/pesquisa) | parcial | `DescribeConstructionQueue`, `DescribeResearchQueue` | top bar texto | Treino tropas inexistente → disabled |
| Menu lateral esquerdo | ausente | Navigator / missões | atalhos reais + disabled | Não duplicar sistemas |
| Menu inferior | parcial | `BetaNavigationBar` | 5 abas | Expandir com shells |
| Chat | ausente | logs/sistema | `IChatProvider` local | Sem conversa fake |
| Labels 17 edifícios | existe | `BuildingDefinition` + level | `BuildingView` TextMesh | Pooling/culling a reforçar |
| HUD bloqueando cidade | OK | pickingMode Ignore no root | City HUD | Manter |
| Responsive 1080×640…1920×1080 | parcial | `ValgorResponsiveUi` | ResponsiveUiAutoTest | Ampliar asserts |
| VIP / poder reino / avatar | ausente | — | disabled/omitir valor | Documentar |
| Castelo/Torre/Muralha densos | parcial (Art Pass 2) | visual builders | dressing | Sem mudar IDs |

## Stack

- City HUD: UIToolkit code-built (`UIDocument` no `CityBootstrap`)
- Bottom nav: `BetaNavigationBar` DontDestroyOnLoad
- Tema provisório: `BetaVisualTheme`
- Ícones procedurais: `ValgorUiIcons` (modais)

## Lacunas P1 a tratar nesta entrega

1. HUD estrutural produto (player + chips + filas + menus + chat shell)
2. Tema central consolidado
3. Labels com culling por distância
4. Densidade urbana / horizonte / iluminação (complemento Art Pass)
5. Testes + build `Valgor-QA-Capital-Final`
