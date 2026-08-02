# 01 — Auditoria (refinamento visual)

**Data:** 2026-08-02  
**Branch:** `master`  
**Base homologada:** `b432d53`  
**Commits preservados:** `0804fc4` · `6331d7b` · `d3845d1` · `b432d53`

## Estado do repositório

| Item | Valor |
|------|--------|
| Branch | master |
| Working tree | limpa |
| Ahead of origin | 3 |
| Build homologado | `builds/windows/Valgor-QA-Capital-Blueprint/Valgor.exe` |
| Screenshots 01–16 | presentes em `city-capital-blueprint-evidence/screenshots/` |
| Suíte | 201/0 |

## Arquitetura atual

```text
CityBootstrap
 └─ CityEnvironmentBuilder.Build → CityRoot
     Terrain / Water / Roads / Districts / Decorations / Vegetation / AmbientFX / TierGroups / Walls
 └─ BuildingSlots (17) — IDs imutáveis via CityCapitalLayoutConfig
 └─ CityCameraController + CityBuildingPointerInput (mask Building)
```

## Controladores

| Componente | Papel |
|------------|--------|
| `CityEnvironmentBuilder` | Todo ambiente procedural |
| `CityEnvironmentTierController` | TierGroups Tier1–6 cumulativos |
| `CityWallVisualController` | Anel muralha por nível wall |
| `CityAmbientQualitySettings` | Low/Medium/High (default Medium) |
| `DragonNestView` | Ocupante Torre; esconde DEPLOYED/INJURED/RECOVERING |

Não existem `CityRoad*` / `CityWater*` / `CityLighting*` / `CityVegetation*` separados — responsabilidade no builder.

## Extensão segura

- Evoluir `CityEnvironmentBuilder` + config `CityCapitalEnvironmentConfig`
- Props/decorações **sem collider** (já padrão em `Part`)
- Raycast só mask `Building` → decoração Default sem collider = seguro
- Não alterar `CityCapitalLayoutConfig` posições
- Não alterar catálogo / migração / requisitos

## Hotspots

1. Muitas primitivas no boot (custo Medium)
2. `renderer.material` instancia materiais
3. Rebuild da muralha ao mudar nível

## Plano

- Bloco A: eixo, praça, Castelo entorno, Torre, relevo, rios, câmera
- Bloco B: props por distrito, vegetação, layers
- Bloco C: luz, atmosfera, tiers densos, qualidade High
- Bloco D: testes, build Visual, screenshots, commit
