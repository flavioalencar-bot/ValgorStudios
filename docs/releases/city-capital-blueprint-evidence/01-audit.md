# 01 — Auditoria técnica (Capital blueprint)

**Projeto:** `C:\Valgor_Studio` · **Branch:** `master`  
**Base:** `0804fc4` · **Fase 5:** `6331d7b`

## Cena principal

`client/Assets/Valgor/City/Scenes/City.unity` — bootstrap em `CityHost` (`CityBootstrap`). Edifícios **não** vivem na cena; são spawnados em runtime.

## Sistema de slots

- Array hardcoded em `CityBootstrap.BuildingLayout` (15 IDs).
- Posições: `CityCapitalLayoutConfig` → `CityLayout.WorldPosition`.
- Save **não** persiste posição (só nível/estado/produção/carteira).

## Catálogo lógico vs blueprint

O jogo tem **15** edifícios oficiais (não 17 IDs). Mapeamento visual:

| Blueprint | ID lógico |
|-----------|-----------|
| Castelo | `castle` |
| Salão dos Heróis | `institute` (posição NW) |
| Torre dos Dragões | `dragon-tower` |
| Hospital / Templo | `hospital` / `temple` |
| Arena | `arena` |
| Campo de Treinamento | `laboratory` (posição SE) |
| Academia | `academy` |
| Mercado / Armazém | `market` / `warehouse` |
| Serraria / Pedreira / Mina / Fazenda | IDs iguais |
| Portão | `wall` (âncora sul + anel visual) |

Sem novos `BuildingDefinition` — sem regressão de gameplay/save.

## Câmera

`CityCameraController` — bounds −22…22, zoom 10–18, pose inicial ampliada.

## Objetos movíveis com segurança

Layout config, ambiente, muralha visual, tiers decorativos, câmera.

## Riscos

- Dessincronizar pads/estradas vs slots.
- Colliders da muralha (proxies).
- Expectativa de 17 IDs vs 15 reais (documentado).
