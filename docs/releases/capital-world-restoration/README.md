# Capital World Restoration

Restauração da Capital estável **antes** do World Foundation quebrado.

## Referência Git

- World Foundation: `4d49b24` — `feat(world): implement mobile capital terrain foundation`
- Base estável (pai): **`8cd4d78`**
- Câmera estável de referência: OverviewZoom≈22 / Pitch 46 / Yaw 40 (ajustada para fill ~70% → ortho **13.5** no Layout V2)

## O que foi desativado no runtime padrão

- `CityTerrainCompositionBuilder` (só com `-capitalWorldPreview` ou `-capitalTerrainAssetGallery`)
- Instanciação automática do catálogo Tripo
- Pads/círculos/planos de demonstração do World Foundation
- Moat em segmentos + WaterPolish ArtPass3 no caminho restored
- Props ArtPass3 / DistrictVisual V1 desalinhados no Layout V2 Preview

## O que foi preservado

- GLB/GLTF/FBX, prefabs, materiais, manifests, TaskIds, `CityTerrainAssetCatalog`
- Pipeline Tripo (pausado — `TripoApiInvocationCount == 0`)
- Layout V2 preview, 17 edifícios, save, gameplay, seleção, testes

## Base mínima

| Camada | Implementação |
|--------|----------------|
| TerrainBase | Solo contínuo grama (`CityCapitalRestoredComposition`) |
| DistantBackdrop | 3 silhuetas baixas atrás da cidade (sem collider/sombra) |
| RiverBase | Canais laterais fora do overview (sem retângulos na tela útil) |

## Câmera

- Overview Layout V2: orthographicSize **13.5**, pitch 48, yaw 36, height 18
- Capital ~70% da área útil; Castelo / Praça / Portão / Muralha / Torre / distritos legíveis

## Flags

```
-cityProgressionQA -capitalLayoutV2Preview
```

**Não** usar `-capitalWorldPreview` neste build.

Galeria de assets: somente `-capitalTerrainAssetGallery`.

## Build

`builds/windows/Valgor-QA-Capital-Restored/Valgor.exe`

## Zero Tripo

- Chamadas: **0**
- Tokens: **0**
- Jobs: **0**
- Retries: **0**
- `TripoApiInvocationCount == 0` (log de captura)

## Screenshots

| Arquivo | Conteúdo |
|---------|----------|
| `01-current-broken.png` | Overview World Foundation (referência) |
| `02-restored-capital.png` | Capital restaurada |
| `03-restored-1080x640.png` | 1080×640 |
| `04-restored-1600x900.png` | 1600×900 |
| `05-restored-1920x1080.png` | 1920×1080 |
| `06-side-by-side.png` | Broken vs Restored |

## Aceite

- **P0 = 0** — sem montanhas soltas de catálogo, sem material preto Tripo, sem fundo cinza vazio, sem borda de mapa no overview, sem vitrine de assets
- **P1 = 0** — sem planos/círculos de teste do World Foundation; sem rio retangular na tela útil

## Commit

`fix(world): restore stable capital composition` (local, sem push)
