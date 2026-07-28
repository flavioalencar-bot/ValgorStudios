# Castle Tier 1 — status

**Status:** ASSET REAL STAGED — integração Unity em andamento / build 0.2.2  
**Source:** `production/City/Castle/source/Castle_Tier1.glb`  
**Staging FBX:** `production/City/Castle/unity_staging/Models/Castle_Tier1.fbx`  
**Unity model:** `client/Assets/Valgor/City/Art/Castle/Models/Castle_Tier1.fbx`

## Escala / pivô (Blender prepare)

Ver `reports/prepare_unity_import.json`:

- scale_factor ≈ 7.65
- footprint ≈ 7.5 m
- height ≈ 4.96 m
- pivot: base center, min Z = 0 (Unity Y-up via FBX)

## Runtime

`CastleRealVisualLoader` → Resources `Valgor/Castle_Tier1`  
Fallback: `CastleTierVisual` (procedural) **somente** se o asset falhar.

## Não é “arte final de produto” indefinidamente

O GLB Tripo/atlas único é o asset real entregue; polish futuro (materiais PBR separados, LODs) fica fora deste passo.
