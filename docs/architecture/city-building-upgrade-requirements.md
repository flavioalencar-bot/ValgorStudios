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
| **Castelo** | Sem gate de Castelo; alvo 2–3 exigem Fazenda Nv.2 + Armazém Nv.2; alvos 4+ sobem em paralelo no catálogo |
| **Serraria** | Castelo ≥ N; Fazenda nos níveis do catálogo (alvo 2–3 → Farm 1; 4–5 → Farm 2) |
| **Pedreira** | Castelo ≥ N; Serraria nos níveis do catálogo |
| **Mina** | Castelo ≥ N; Pedreira nos níveis do catálogo |
| **Academia** | Castelo ≥ N; Armazém nos níveis do catálogo |

## Integração

- `CityController.GetCastleLevel()` → **somente** nível do edifício `castle` persistido na cidade (não PlayerLevel / `BetaProgress`)
- `CityController.CanUpgrade` / `GetUpgradeBlockReason` / `GetDependencyChecks` / `TryGetBuildingByDefinitionId`
- Painel Atualizar: Pré-requisitos (vermelho se não cumprido + botão **Ir**) + Recursos; **Atualizar** desabilitado se bloqueado
- **Ir** seleciona/centraliza o edifício exigido
- `SyncBetaProgress` espelha Castelo cidade → beta (nunca o inverso na validação de upgrade)

## Outros (catálogo)

Mine/quarry, academy deps, dragon-tower + `research.gatherBoost`, etc. Fallback genérico: Castelo ≥ nível-alvo.
