# UX contextual de edifícios (Beta 0.1)

O edifício é a interface principal. Não há menu central de construções.

## Fluxo

1. Clique/toque no edifício → destaque + câmera suave (`CityCameraController.FocusOn`)
2. Menu contextual circular ao lado (`BuildingContextMenu` + `BuildingContextMenuPositioner`)
3. Ação → painel (Detalhes / Atualizar / Abrir / Dragões) ou execução imediata (Coletar / Alimentar)
4. Clique fora (chão) fecha; outro edifício troca o contexto
5. Arrastar a câmera (esquerdo/toque) **não** seleciona prédio

## Entregas

### 1ª — Castelo / Fazenda / Armazém

| Edifício | Ações |
|----------|--------|
| Castelo | Detalhes, Atualizar |
| Fazenda | Coletar, Detalhes, Atualizar |
| Armazém | Abrir, Detalhes, Atualizar |

### 2ª — Serraria / Pedreira / Mina / Academia

| Edifício | Ações |
|----------|--------|
| Serraria / Pedreira / Mina | Coletar, Detalhes, Atualizar |
| Academia | Detalhes, Atualizar |

### 3ª — Arena / Hospital / Torre / Templo / Mercado / Laboratório

| Edifício | Ações |
|----------|--------|
| Arena / Hospital / Templo / Mercado / Laboratório | Abrir, Detalhes, Atualizar |
| Torre dos Dragões | Dragões, Alimentar, Detalhes, Atualizar |

`SupportBuildingRules` + `ProductionBuildingDetails` alimentam Detalhes. Torre reutiliza `IDragonGateway` (sem duplicar lógica).

Fora do escopo: PvP, comércio entre jogadores, árvore de pesquisa nova, religião/facção, sistema de feridos.

## Painel Atualizar

Pré-requisitos data-driven + **Ir** + recursos + construtor + Concluir Agora. Ver `city-building-upgrade-requirements.md`.

## Evidências

- `docs/releases/beta-0.1-evidence/ux-contextual/` (entregas 1–2 + gates)
- `docs/releases/beta-0.1-evidence/ux-contextual-full/` (bloco completo cidade)
