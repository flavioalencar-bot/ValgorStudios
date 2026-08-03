# 03 — Preview (Etapa A)

## Flag CLI

```text
Valgor.exe -cityProgressionQA -capitalLayoutV2Preview -screen-width 1600 -screen-height 900
```

## Comportamento

| Item | Preview V2 |
|---|---|
| Posições | `CityCapitalLayoutV2` |
| Save | **não** migra / **não** persiste versão |
| Anchors | esferas + TextMesh com BuildingId |
| Screenshots | `docs/releases/capital-layout-v2/screenshots/` |
| Banner | `Capital Layout V2 Preview — sem migração de save` |

## Fluxo

1. `CityProgressionQa.IsLayoutV2Preview` detecta a flag.
2. `CityBootstrap` chama `UsePreviewV2()` **antes** de spawnar prédios.
3. `CapitalLayoutV2PreviewDriver` captura overview, setores e resoluções mobile/desktop.

## Comparação V1/V2

- Diagrama offline: `screenshots/12-v1-v2-comparison.png`
- Player V1: mesma build **sem** `-capitalLayoutV2Preview` → `01-v1-overview.png`
- Player V2: com flag → `02-v2-overview.png` …

## Status

```text
Capital Layout V2 disponível para aprovação visual.
```

V2 **não** é layout oficial até Etapa B.
