# Dragons Foundation

Sistema de dragões em `Assets/Valgor/Dragons`.

## Objetivo

**Fase 1:** Castelo Nv.20 → ovo → incubação → nascimento Nv.1.  
**Fase 2:** progressão Nv.1→30 (XP, alimentação, vínculo, energia, saúde, caps, rituais, timers, aceleradores).

## Limite

Integração via `IDragonGateway`. Sem combate completo, PvP, montaria ou múltiplos dragões nesta fase.

## Jornada do ovo (Fase 1)

```text
LOCKED → UNLOCKED (Castelo ≥ 20) → MISSION_ACTIVE → EGG_OWNED → INCUBATING → BORN
```

## Progressão (Fase 2)

```text
Nv.1 … Nv.30
XP via alimentação
Caps: Castelo + Torre dos Dragões
Rituais ao atingir 6 / 11 / 16 / 21 / 26
Estágios: Hatchling→Juvenile→Adult→Elder→Ancient
```

| API | Papel |
|-----|--------|
| `SyncBuildingLevels` | Caps Castelo/Torre |
| `TryStartLevelUp` | Inicia evolução ou ritual |
| `TryInstantCompleteLevelUp` | Acelerador (diamantes) |
| `DescribeDragonProgression` | Texto HUD |

Persistência: `valgor.dragons.v5` (migra `v4` automaticamente).

## Seed

- `dragon-ember-1` (`ember-whelp`) — LOCKED até a jornada; após nascimento, progressão Fase 2.
