# Pré-requisitos de evolução de edifícios

Extensão do upgrade existente (sem novos edifícios). Um prédio só evolui quando cumprir:

1. Recursos (`HasUpgradeFunds` / `BuildingUpgradeRequirements`)
2. Construtor livre (fila 1/1)
3. Castelo no nível mínimo (`BuildingRequirementCatalog`)
4. Outros edifícios no nível mínimo
5. Pesquisa/desbloqueio, quando listado
6. Nenhuma evolução já em andamento no mesmo prédio (`BuildingState.Upgrading`)

## Tipos data-driven

| Tipo | Papel |
|------|--------|
| `BuildingLevelRequirement` | Outro prédio ≥ Nv.X |
| `BuildingUnlockRequirement` | Chave de pesquisa/desbloqueio |
| `BuildingUpgradeRequirement` | Pacote castelo + prédios + unlocks |
| `BuildingUpgradeDefinition` | Default + overrides por nível-alvo (`DynamicCastleLevel = -1` → alvo) |
| `BuildingRequirementCatalog` | Tabela por `definitionId` |
| `BuildingRequirementEvaluator` | Avalia checks puro (injetável em testes) |

## Regras iniciais (obrigatórias)

| Edifício | Evoluir para Nv.N |
|----------|-------------------|
| **Fazenda** | Castelo ≥ N |
| **Armazém** | Castelo ≥ N; Nv.2+ exige Fazenda (Nv.2 no catálogo para alvo 2–4; Nv.3 para alvo 5) |
| **Castelo** | Fazenda e Armazém nos níveis do catálogo (sem gate de Castelo) |

## Integração

- `CityController.CanUpgrade` / `GetUpgradeBlockReason` / `GetDependencyChecks` / `TryGetBuildingByDefinitionId`
- Painel Atualizar: Pré-requisitos (vermelho se não cumprido + botão **Ir**) + Recursos; **Atualizar** desabilitado se bloqueado
- **Ir** seleciona/centraliza o edifício exigido

## Outros (catálogo)

Mine/quarry, academy deps, dragon-tower + `research.gatherBoost`, etc. Fallback genérico: Castelo ≥ nível-alvo.
