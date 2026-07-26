# Placeholders de heróis (Addressables)

Modelos 3D finais ainda não estão no repositório. Use dummies com as chaves abaixo e substitua quando a arte estiver pronta.

## Prefabs

| HeroId | Addressable Key | Status visual |
|--------|-----------------|---------------|
| HERO_VORTEX_000 | `heroes/HERO_VORTEX_000/prefab` | Pipeline pronta → `Characters/Vortex/Prefabs/Vortex_Hero.prefab` (fallback até FBX) |
| HERO_ELYRA_001 | `heroes/HERO_ELYRA_001/prefab` | Placeholder |
| HERO_CONSORTE_002 | `heroes/HERO_CONSORTE_002/prefab` | Placeholder |
| HERO_SOMBRA_003 | `heroes/HERO_SOMBRA_003/prefab` | Placeholder |
| HERO_LYRIANNE_004 | `heroes/HERO_LYRIANNE_004/prefab` | Placeholder |
| HERO_AKEMI_005 | `heroes/HERO_AKEMI_005/prefab` | Placeholder |
| HERO_SERENA_006 | `heroes/HERO_SERENA_006/prefab` | Placeholder |
| HERO_ABISMO_007 | `heroes/HERO_ABISMO_007/prefab` | Placeholder |
| HERO_ZAHARA_008 | `heroes/HERO_ZAHARA_008/prefab` | Placeholder |
| HERO_NYXARA_009 | `heroes/HERO_NYXARA_009/prefab` | Placeholder |
| HERO_VESPERA_010 | `heroes/HERO_VESPERA_010/prefab` | Placeholder |

## Retratos / VFX / SFX / Skins

- Retrato: `heroes/{heroId}/portrait`
- VFX especial: `vfx/special/POWER_{heroId}`
- SFX especial: `sfx/special/POWER_{heroId}`
- Skin padrão: `heroes/{heroId}/skins/default/model`
- Skin real da Consorte: `heroes/HERO_CONSORTE_002/skins/royal/model`

## Como substituir

1. Importar o modelo final em `Characters/<Hero>/`.
2. Marcar o prefab como Addressable com a chave acima.
3. Atualizar materiais/LOD no mesmo endereço.
4. Não alterar IDs internos nem o `heroes.seed.json`.
