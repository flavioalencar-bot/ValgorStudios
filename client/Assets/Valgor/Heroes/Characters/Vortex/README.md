# Vortex — herói 3D real (pipeline)

Fonte de verdade: [`docs/game-design/heroes/VALGOR_SPRINT_HERO_REAL_VORTEX.md`](../../../../docs/game-design/heroes/VALGOR_SPRINT_HERO_REAL_VORTEX.md)

## Estado atual

**Modelo real integrado** com rig Humanoid, 16 clips mínimos, espada dracônica e VFX do Domínio do Rei.

| Item | Status |
|------|--------|
| `Models/Vortex_LOD0.fbx` | Presente (malha aprovada + armature + animações) |
| `Models/Vortex_DragonSword.fbx` | Presente |
| `Prefabs/Vortex_Hero.prefab` | Modelo real (`usingTechnicalFallback=false`) |
| Avatar | Humanoid (Create From This Model) |
| Animator | `Vortex_Animator.controller` com Idle…Death |
| Preview | `Scenes/HeroesDemo.unity` |

Produção Blender: `production/Vortex/` (`rig_animate_weapon_vortex.py`).

## Onde visualizar

1. Abrir `C:\Valgor_Studio\client` no Unity 6000.0.58f2
2. Cena: `Assets/Valgor/Heroes/Scenes/HeroesDemo.unity`
3. Play Mode — Idle automático, drag/zoom, botão de poder → `Special_Power` + aura 10s
4. Prefab: `Assets/Valgor/Heroes/Characters/Vortex/Prefabs/Vortex_Hero.prefab`

## Menus Unity

```text
Valgor → Heroes → Vortex → Create Folder Scaffold
Valgor → Heroes → Vortex → Validate Source Assets
Valgor → Heroes → Vortex → Build Vortex Prefab
Valgor → Heroes → Vortex → Open Vortex Preview
Valgor → Heroes → Validate Demo In Play Mode
```

## Assets

| Item | Caminho |
|------|---------|
| LOD0 (corpo + rig + clips) | `Models/Vortex_LOD0.fbx` |
| Espada | `Models/Vortex_DragonSword.fbx` |
| Animator | `Animations/Vortex_Animator.controller` |
| Prefab | `Prefabs/Vortex_Hero.prefab` |
| Addressable | `heroes/HERO_VORTEX_000/prefab` |

### Requisitos atendidos

- Humanoid Avatar, altura ~2,05 m, scale 1, pivô nos pés, olhar +Z
- Sockets (mão/costas/quadril/DragonLink/VFX) no prefab
- Espada em `Socket_RightHand` (troca para Back/Hip via `HeroVisualController.AttachWeaponTo`)
- Clips: Idle, Idle_Combat, Walk, Run, Turn_*, Attack_*, Heavy_Attack, Special_Power, Hit_*, Stun, Victory, Defeat, Death
- Domínio do Rei: animação + aura/runas douradas ~10s (dados de gameplay do seed **inalterados**)

### Pendências de arte (não bloqueantes do MVP)

- LOD1/LOD2
- Conjunto canônico de texturas PBR separadas (hoje usa embutida do Tripo)
- Refino manual de pesos / mocap de qualidade final

## Integração

- Catálogo: `HERO_VORTEX_000.PrefabAddress` → `heroes/HERO_VORTEX_000/prefab`
- Dados de gameplay (nome, facção, poder, cooldowns) **não** são alterados
- Fallback técnico só se o FBX for removido
