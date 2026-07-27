# UX contextual de edifícios (Beta 0.1)

O edifício é a interface principal. Não há menu central de construções.

## Fluxo

1. Clique no edifício → destaque + câmera suave (`CityCameraController.FocusOn`)
2. Menu contextual ao lado (`BuildingContextMenu` + `BuildingContextMenuPositioner`)
3. Ação → painel específico (direita) ou execução imediata (Coletar / Enviar)
4. Clique fora (chão/céu) fecha; outro edifício troca o contexto

## Tipos

| Tipo | Papel |
|------|--------|
| `BuildingContextAction` | Enum + info de botão |
| `BuildingContextMenu` | Lista compacta de ações |
| `BuildingContextMenuPositioner` | World→UI com área segura |
| `BuildingSelectionPresenter` | Orquestra seleção/câmera/menu/painel |

## Ações × edifícios

| Ação | Quando aparece |
|------|----------------|
| Detalhes | Sempre |
| Atualizar / Construir | Sempre (desabilitado se bloqueado) |
| Coletar / Produzir | Catálogo de produção |
| Treinar | Arena |
| Pesquisar | Laboratório / Academia |
| Abrir / Enviar | Torre dos Dragões |

Reutiliza `TryUpgradeSelected`, `CollectSelected`, dragões e navegação ao mapa.
