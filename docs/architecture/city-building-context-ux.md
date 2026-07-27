# UX contextual de edifícios (Beta 0.1)

O edifício é a interface principal. Não há menu central de construções.

## Fluxo

1. Clique/toque no edifício → destaque + câmera suave (`CityCameraController.FocusOn`)
2. Menu contextual circular ao lado (`BuildingContextMenu` + `BuildingContextMenuPositioner`)
3. Ação → painel (Detalhes / Atualizar / Abrir) ou execução imediata (Coletar)
4. Clique fora (chão) fecha; outro edifício troca o contexto
5. Arrastar a câmera (esquerdo/toque) **não** seleciona prédio

## Primeira entrega (2026-07-27)

| Edifício | Ações |
|----------|--------|
| Castelo (`castle`) | Detalhes, Atualizar |
| Fazenda (`farm`) | Coletar, Detalhes, Atualizar |
| Armazém (`warehouse`) | Abrir, Detalhes, Atualizar |

### Painel Atualizar

Nome, níveis, benefício, duração, **pré-requisitos** (Castelo / prédios / pesquisa com ✓/✗), requisitos de recursos (Ouro/Comida/Madeira/Pedra/Ferro/Essência com ✓/✗), botões **Atualizar**, **Concluir Agora** (diamantes), **Fechar**.

Ver `city-building-upgrade-requirements.md` para o catálogo data-driven (Fazenda/Armazém/Castelo + botão **Ir**).

### Mundo

- Indicador de coleta na Fazenda (toque direto + menu)
- Barra/tempo/ícone de construção sobre o prédio
- Armazém: capacidade + proteção (`WarehouseRules`)

## Tipos

| Tipo | Papel |
|------|--------|
| `BuildingContextAction` | Enum + info de botão |
| `BuildingContextMenu` | Botões circulares Valgor |
| `BuildingContextMenuPositioner` | World→UI com área segura |
| `BuildingSelectionPresenter` | Orquestra seleção/câmera/menu/painel |
| `BuildingUpgradeRequirements` | Custos de recursos + Concluir Agora |
| `BuildingRequirementCatalog` / `Evaluator` | Pré-requisitos Castelo/prédios/unlock |
| `WarehouseRules` | Capacidade/proteção |

## Evidências smoke

`ux-01`…`ux-10` via `CheckpointSmokeDriver` + `scripts/capture-checkpoint-evidence.ps1`.
