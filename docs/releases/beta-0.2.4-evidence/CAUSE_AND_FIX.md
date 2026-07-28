# Castle GLB — causa e correção (Beta 0.2.4)

## Causa exata

1. **Roundtrip Blender → FBX** invertia/corrompia UV no Unity (V-flip clássico).
2. **`CastleTiersPrefabBuilder` antigo** forçava um único `M_Castle_TierN_URP` em todos os slots.
3. **Prefabs Tier1 (e possivelmente outros) ficaram com `PrefabInstance` preso ao FBX legado**
   (`guid ff9399bd…`) mesmo após copiar `.glb` — `SaveAsPrefabAsset` preservava o link FBX.
   Isso manteve textura embaralhada do FBX na City enquanto a cena isolada às vezes já usava GLB.
4. **`CastleRealVisualLoader` forçava `localScale = Vector3.one`**, anulando a escala progressiva do prefab
   (Tier6 aparecia microscópico na City).
5. **`BuildingView.ApplyColors` → `RuntimeSafeMaterials`** podia substituir materiais do asset real.

## Importador

- **Unity glTFast** (`com.unity.cloud.gltfast`)
- Fonte: `production/City/Castle/source/Castle_TierN.glb` (Tripo original, sem reexport Blender)
- Unity: `Assets/Valgor/City/Art/Castle/Models/Castle_TierN.glb`
- FBX removidos do pipeline

## Materiais / submeshes (diagnóstico Tripo)

| Tier | Meshes | Slots / submeshes | Material | UV0 |
|-----:|-------:|------------------:|----------|-----|
| 1–6  | 1      | 1                 | 1 atlas BaseColor | OK |

Materiais **glTFast** preservados (fallback URP/Lit só se shader inválido).

## Correções

- glTFast + GLB nativo
- Apagar FBX + deletar prefabs antes de regenerar (sem nested FBX)
- `Instantiate` em vez de `PrefabUtility.InstantiatePrefab` no builder
- Não resetar escala no loader
- `ApplyColors` no-op para `definitionId == castle`
- Cena `CastleImportValidation.unity` + capturas isoladas
- Escala no Transform (footprint 7.5→10.8 m)

## Validação

- Previews Blender Tripo: `docs/releases/beta-0.2.4-evidence/tripo-source-previews/`
- Isolados: `docs/releases/beta-0.2.4-evidence/isolated-tiers/`
- City Tier1/Tier6: `docs/releases/beta-0.2.4-evidence/city/`
- Smoke checkpoint exit 0
- Build: `builds/windows/Valgor-Beta-0.2.4/Valgor.exe`
