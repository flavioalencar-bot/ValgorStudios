# Vortex — herói 3D real (pipeline)

Fonte de verdade: [`docs/game-design/heroes/VALGOR_SPRINT_HERO_REAL_VORTEX.md`](../../../../docs/game-design/heroes/VALGOR_SPRINT_HERO_REAL_VORTEX.md)

## Estado atual

A pipeline de importação/validação/prefab está pronta.

**O arquivo 3D final de Vortex ainda não está no repositório.**  
Enquanto isso, `Prefabs/Vortex_Hero.prefab` usa fallback técnico (`HumanoidDummy`) e o preview continua funcionando.

## Menus Unity

```text
Valgor → Heroes → Vortex → Create Folder Scaffold
Valgor → Heroes → Vortex → Validate Source Assets
Valgor → Heroes → Vortex → Build Vortex Prefab
Valgor → Heroes → Vortex → Open Vortex Preview
```

Ao importar `Vortex_LOD0.fbx` (ou `Vortex.fbx` / `Vortex.glb`) em `Models/`, o postprocessor reconstrói o prefab automaticamente.

## O que colocar (arte externa)

| Item | Caminho / nome |
|------|----------------|
| Modelo LOD0 | `Models/Vortex_LOD0.fbx` (ou `Vortex.fbx` / `Vortex.glb`) |
| LOD1 / LOD2 | `Models/Vortex_LOD1.fbx`, `Vortex_LOD2.fbx` |
| Texturas | ver `Textures/PLACE_TEXTURES_HERE.md` |
| Animações | ver `Animations/PLACE_ANIMATIONS_HERE.md` |
| Prefab gerado | `Prefabs/Vortex_Hero.prefab` |
| Addressable | `heroes/HERO_VORTEX_000/prefab` |

### Requisitos do modelo

- Humanoid Avatar válido
- Altura ~2,05 m, scale 1, pivô nos pés, olhar +Z
- T-pose ou A-pose
- Produzido fora do Cursor (Blender / Maya / Character Creator / etc.)

## Integração

- Catálogo: somente `HERO_VORTEX_000.PrefabAddress` → `heroes/HERO_VORTEX_000/prefab`
- Dados de gameplay (nome, facção, poder, cooldowns) **não** são alterados
- HeroesDemo: preview 360°, drag/zoom, Idle, botão de poder especial dispara `Special_Power` quando houver clip

## Addressables (passo no Unity Editor)

O projeto ainda não tem `AddressableAssetSettings` criado. Depois de abrir o client no Editor:

1. `Window → Asset Management → Addressables → Groups`
2. Criar settings se pedido
3. Marcar `Prefabs/Vortex_Hero.prefab` com address `heroes/HERO_VORTEX_000/prefab`

Até lá, o preview Editor resolve o prefab via `AssetDatabase` / shell local.
