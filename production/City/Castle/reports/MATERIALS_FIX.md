# Diagnóstico materiais — Castle Tier 1

**Data:** 2026-07-28  
**Source:** `production/City/Castle/source/Castle_Tier1.glb`

## Conteúdo do GLB

| Canal | Presente | Nota |
|-------|----------|------|
| Material | Sim (1) | Tripo single material |
| Base Color / Albedo | Sim | Textura embutida 4096×4096 sRGB |
| Normal | Não | — |
| Metallic map | Não | Metallic constante **0** |
| Roughness map | Não | Roughness constante **0.5** |
| Occlusion | Não | — |
| Emission | Não | Strength 0 |
| Texturas externas | Não | Packed no GLB |

**Smoothness Unity:** `1 - 0.5 = 0.5`

## Causa do castelo branco/cinza

`BuildingView.ApplyColors` aplicava `RuntimeSafeMaterials` (cor sólida) em **todos** os renderers do Visual, apagando o Base Map do asset real.

## Correção

1. Extrair Base Color → `Assets/Valgor/City/Art/Castle/Textures/Castle_Tier1_BaseColor.jpg`
2. Material URP/Lit `M_Castle_Tier1_URP.mat`
3. Prefab Resources reatribuído
4. Renderers `Castle_Tier1*` isentos do tint/recolor
