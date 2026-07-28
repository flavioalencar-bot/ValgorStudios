# Prefab de staging — Castle_Tier1_Visual

**Ainda não criar o prefab Unity na City** até o asset real + ordem expressa.

## Contrato do prefab (quando existir)

| Item | Valor |
|------|--------|
| Nome | `Castle_Tier1_Visual` |
| Path | `Assets/Valgor/City/Art/Castle/Prefabs/Castle_Tier1_Visual.prefab` |
| Root | equivale ao filho `Visual` atual |
| Sem | `BuildingView`, collider de seleção, lógica de upgrade |
| Com | mesh/hierarquia do FBX/GLB + materiais |

## Runtime

```text
BuildingSlot (castle)
  └─ Visual          ← trocar conteúdo / prefab instance
       └─ (mesh real OU CastleTier1 procedural fallback)
  BuildingView
  BoxCollider
```

Loader previsto (futuro, sob ordem):

1. Se `Castle_Tier1` importado e válido → instancia prefab sob `Visual`
2. Senão → `CastleTierVisual.Build(..., visualTier: 1)` (fallback)

Não marcar como concluído enquanto `blocked: true` no manifesto.
