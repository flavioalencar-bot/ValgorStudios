# 02 — Arquitetura ambiental

## Hierarquia

```text
CityRoot
├── Terrain
├── Water
├── Roads / ImperialRoad / MainRoads / SecondaryRoads
├── Walls / CityFortifications
├── Districts / Royal|Dragon|Military|Economic|Industrial|Sacred|Rural
├── Buildings (slots no bootstrap)
├── Decorations
├── Vegetation
├── AmbientFX
├── NPCs
├── Navigation
└── TierGroups / Tier1..Tier6
```

## Componentes

| Nome | Papel |
|------|--------|
| `CityEnvironmentBuilder` | Construção do CityRoot |
| `CityDistrictVisualBuilder` | Props e identidade por distrito |
| `CityCapitalEnvironmentConfig` | Densidades / Medium-High |
| `CityAmbientQuality` | Gates Low/Medium/High |
| `CityEnvironmentTierController` | Ativação cumulativa de tiers |
| `CityWallVisualController` | Muralha visual única |

## Regras

- Layout lógico dos 17 edifícios permanece em `CityCapitalLayoutConfig`.
- Decoração: sem collider + layer Ignore Raycast.
- Raycast de seleção: apenas layer Building.
