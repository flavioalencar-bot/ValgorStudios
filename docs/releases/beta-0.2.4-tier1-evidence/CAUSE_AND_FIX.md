# Castle Tier 1 — causa e correção (Beta 0.2.4-Tier1)

## Causa exata do defeito

Não era o GLB Tripo. Diagnóstico Blender no source:

- 1 mesh, 1 material, 1 imagem Base Color, UV0 presente
- Preview Blender bate com o preview Tripo

O defeito vinha do **pipeline Unity**:

1. Roundtrip Blender→FBX ou PrefabInstance preso a FBX legado (UV V-flip / materiais quebrados)
2. Builder que **forçava um único material** em todos os slots
3. `BuildingView.ApplyColors` → `RuntimeSafeMaterials` podia sobrescrever materiais glTF
4. Loader podia anular escala do prefab

## Importador

`com.unity.cloud.gltfast` — GLB nativo, sem FBX.

Fonte: `production/City/Castle/source/Castle_Tier1.glb`

## Contagens (Unity pós-import)

| Item | Valor |
|------|-------|
| Meshes / renderers | 1 |
| Submeshes | 1 |
| Material slots | 1 |
| Material | `tripo_node_…_material` (Shader Graphs/glTF-pbrMetallicRoughness) |
| Rematerializado | **não** |
| ApplyColors no Castelo | **no-op** |
| Footprint | ~7,5 m |
| Escala local | ~7,62 (só Transform) |

## Correções (só Tier 1)

- Cópia GLB → `Assets/Valgor/City/Art/Castle/Models/Castle_Tier1.glb`
- Prefab sem rematerializar; materiais glTFast intactos
- Cena isolada: `Assets/Valgor/City/Scenes/CastleImportValidation.unity` (sem City/BuildingView/seleção)
- City: `CastleRealVisualLoader.ResolveTier` **sempre retorna 1** até Tiers 2–6 serem aprovados
- Build isolada: `builds/windows/Valgor-Beta-0.2.4-Tier1/`

## Evidências

- Tripo/Blender: `docs/releases/beta-0.2.4-tier1-evidence/tripo-tier1-blender.png`
- Isolado Unity: `docs/releases/beta-0.2.4-tier1-evidence/tier1-isolated.png`
- City: `docs/releases/beta-0.2.4-tier1-evidence/tier1-city.png` (após smoke)
- Inspect JSON: `tier1-unity-inspect.json`

## Não feito

Tiers 2–6 **não** integrados nesta entrega.
