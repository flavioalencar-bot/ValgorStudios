# Art / Capital — Assets de alta fidelidade

Esta pasta recebe **apenas** arte definitiva da Capital.

- Lógica de gameplay: `Assets/Valgor/City/**`
- Proxies procedurais: permanecem até o slot do catálogo ter conteúdo
- Naming: `VAL_Capital_<Categoria>_<Nome>_<Variante>`
- Pack inicial: `CapitalArtPack_Core_v1` (ver `docs/art/capital-asset-manifest.md`)

Não colocar scripts de lógica, colliders de gameplay ou `BuildingView` nos prefabs artísticos.

Integração: `CityArtAssetCatalog` → `CityArtAssetResolver` → `CityArtReplacementController`.
