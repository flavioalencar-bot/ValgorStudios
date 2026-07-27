# UX contextual de edifícios (Beta 0.1)

O edifício é a interface principal. Não há menu central de construções.

## Fluxo

1. Clique/toque no edifício → destaque + câmera suave (`CityCameraController.FocusOn`)
2. Menu contextual circular ao lado (`BuildingContextMenu` + `BuildingContextMenuPositioner`)
3. Ação → painel (Detalhes / Atualizar / Abrir) ou execução imediata (Coletar)
4. Clique fora (chão) fecha; outro edifício troca o contexto
5. Arrastar a câmera (esquerdo/toque) **não** seleciona prédio

## Entregas

### 1ª — Castelo / Fazenda / Armazém

| Edifício | Ações |
|----------|--------|
| Castelo (`castle`) | Detalhes, Atualizar |
| Fazenda (`farm`) | Coletar, Detalhes, Atualizar |
| Armazém (`warehouse`) | Abrir, Detalhes, Atualizar |

### 2ª — Serraria / Pedreira / Mina / Academia

| Edifício | Ações |
|----------|--------|
| Serraria (`lumbermill`) | Coletar, Detalhes, Atualizar |
| Pedreira (`quarry`) | Coletar, Detalhes, Atualizar |
| Mina (`mine`) | Coletar, Detalhes, Atualizar |
| Academia (`academy`) | Detalhes, Atualizar |

Detalhes de produção (`ProductionBuildingDetails`): taxa/h, armazenado, capacidade, tempo até lotar, bônus, próximo nível.

Fora desta etapa: Arena, Hospital, Torre dos Dragões.

### Painel Atualizar

Nome, níveis, benefício, duração, **pré-requisitos** (Castelo / prédios / pesquisa com ✓/✗ + **Ir**), recursos, **Atualizar** / **Concluir Agora** / **Fechar**.

Ver `city-building-upgrade-requirements.md`.

### Mundo

- Indicador de coleta nos produtores
- Barra/tempo de construção sobre o prédio
- Armazém: capacidade + proteção (`WarehouseRules`)

## Tipos

| Tipo | Papel |
|------|--------|
| `BuildingContextAction` | Enum + info de botão |
| `BuildingContextMenu` | Botões circulares Valgor |
| `BuildingContextMenuPositioner` | World→UI com área segura |
| `BuildingSelectionPresenter` | Orquestra seleção/câmera/menu/painel |
| `BuildingDetailsViewModel` | Texto do painel Detalhes |
| `ProductionBuildingDetails` | Bloco rico de produção |
| `BuildingUpgradeRequirements` | Custos de recursos + Concluir Agora |
| `BuildingRequirementCatalog` / `Evaluator` | Pré-requisitos |
| `WarehouseRules` | Capacidade/proteção |

## Evidências smoke

`ux-01`…`ux-14` (1ª + gates) e `ux-15`…`ux-24` (2ª entrega) via `CheckpointSmokeDriver`.
