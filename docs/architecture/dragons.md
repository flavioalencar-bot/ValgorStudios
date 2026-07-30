# Dragons Foundation

Sistema de dragões em `Assets/Valgor/Dragons`.

## Objetivo

**Fase 1:** Castelo Nv.20 → ovo → incubação → nascimento Nv.1.  
**Fase 2:** progressão Nv.1→30.  
**Fase 3:** habilidades + combate PvE suporte.  
**Fase 4:** montaria estratégica + vínculo com herói + presença na marcha.

## Limite

Via `IDragonGateway`. Sem PvP, voo manual, múltiplos dragões ou combate aéreo livre.

## Montaria (Fase 4)

```text
Vínculo herói compatível → treinar → equipar montaria
→ marcha PvE (dragão visível no MarchArmyView)
→ bônus MountBondLevel no poder de suporte
→ recall + pontos de vínculo de montaria
```

| API | Papel |
|-----|--------|
| `TryCreateMountBond` / `TryClearMountBond` | Vínculo herói |
| `TryTrainMountBond` | Treino (comida/essência) |
| `TryEquipMount` / `TryUnequipMount` | Formação montada |
| `DescribeMountBond` | HUD |
| `TryGetMarchDragonPresence` | Presença visual no mapa |

Persistência: `valgor.dragons.v7` (migra `v6`/`v5`/`v4`).

## Seed

- `dragon-ember-1` (`ember-whelp`)
- Montadores beta: Vortex, Elyra (Nv.6+), Vespera (Nv.11+)
