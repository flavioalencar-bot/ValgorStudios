# 02 — Arquitetura

Camadas: (1) horizonte/montanhas (2) terreno jogável/rios/penhascos (3) gameplay 17 edifícios.

Sistemas: CityCapitalWorldConfig, CityTerrainAssetCatalog, CityTerrainAssetResolver, CityTerrainCompositionBuilder, CityHorizonBuilder, CityWaterCompositionBuilder, CityVegetationCompositionBuilder.

Entry: CityEnvironmentBuilder.Build → CityTerrainCompositionBuilder quando Enabled.
