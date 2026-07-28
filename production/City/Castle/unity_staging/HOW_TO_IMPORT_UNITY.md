# Unity import — Castle Tier 1

**Status:** `BLOQUEADO POR ASSET REAL`

## Staging

- Models: `production/City/Castle/unity_staging/Models/`
- Prefab shell notes: `production/City/Castle/unity_staging/PrefabStaging/`

## Target (somente após asset + ordem)

- Models: `client/Assets/Valgor/City/Art/Castle/Models/`
- Prefab: `client/Assets/Valgor/City/Art/Castle/Prefabs/Castle_Tier1_Visual.prefab`

## Integração

Substituir **apenas** o filho `Visual` do slot `castle`.

Não alterar:

- `definitionId`
- `BuildingSlot` / `BuildingView`
- collider / seleção / Detalhes / Atualizar / save

Fallback: `CastleTierVisual` (procedural) permanece até o asset real carregar.

## Regras

- Só após validação de escala/pivô.
- Nunca promover o procedural como arte aprovada.
- Não sobrescrever assets com `VALGOR_APPROVED_ART` / aprovados.
