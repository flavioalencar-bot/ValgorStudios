# 04 — Labels dos edifícios

## Formato

```text
{nível} {nome}
exemplo: 28 Castelo
```

Em upgrade: `{nível}→{nível+1} {nome}`.

## Sistema

- `CityBuildingWorldLabel` no TextMesh `Name`
- `CityBuildingLabelManager` — culling por `orthographicSize`
- Janela: `BuildingLabelMinOrtho` … `BuildingLabelMaxOrtho`
- LabelPlate sem collider (Ignore Raycast / Destroy collider)
- Seleção força visibilidade (`SetForceVisible`)
- Pooling: registro em lista reutilizada (sem recriar TextMesh)

## Contagem

Exatamente **17** labels oficiais (= `BuildingIdMigration.OfficialCount`).
Decoração urbana **não** gera labels.
