# 01 — Arquitetura da pipeline

```text
BuildingLogicRoot (slot)
├── BoxCollider / BuildingView / regras
└── BuildingVisualRoot ("Visual")
    ├── proxy procedural (ativo se HF ausente)
    ├── Castle_TierN_Real (castelo)
    └── ArtHF (prefab definitivo, se disponível)
```

Componentes:

| Peça | Papel |
|------|--------|
| `CityArtAssetCatalog` | SO / runtime slots BuildingId → prefabs |
| `CityArtAssetCatalogLogic` | regras testáveis |
| `CityArtAssetResolver` | qualidade → modo (HF/Med/Low/Fallback) |
| `CityArtReplacementController` | aplica HF, oculta proxy, strip colliders |
| `CityArtPrefabValidation` | bloqueia scripts de gameplay |

Fallback: conteúdo ausente → proxy permanece. Jogo nunca fica sem visual.
