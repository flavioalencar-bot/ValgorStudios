# Dragons Foundation

Sistema de dragões em `Assets/Valgor/Dragons`.

## Objetivo

**Fase 1:** Castelo Nv.20 → ovo → incubação → nascimento Nv.1.  
**Fase 2:** progressão Nv.1→30.  
**Fase 3:** habilidades + combate PvE suporte.  
**Fase 4:** montaria estratégica + vínculo com herói + presença na marcha.  
**Fase 5:** raid PvE a cidades NPC + modos Support/Assault/Guard + defesa local (D030).

## Limite

Via `IDragonGateway`. Sem PvP multiplayer, voo manual, múltiplos dragões ou combate aéreo livre.

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

## Raid / modos (Fase 5)

```text
Modo Support|Assault|Guard (Torre)
→ marcha a ash-keep / ruin-citadel
→ engajar → resolver → saque + feridos + relatório
→ Guard na home: muralha + GetHomeGuardPower
```

| API | Papel |
|-----|--------|
| `TrySetBattleRole` / `DescribeBattleRole` | Modo de combate |
| `GetHomeGuardPower` | Defesa da cidade |
| `WorldMapSession.TryEngageSelectedCityRaid` | Engajar raid |
| `TryResolveSelectedCityRaid` | Resolver + saque |
| `TrySimulateHomeDefense` | Defesa provisória |
| `LastBattleReport` | Relatório persistido |

Persistência: `valgor.dragons.v8` (migra `v7`/`v6`/`v5`/`v4`).

## Seed

- `dragon-ember-1` (`ember-whelp`)
- Montadores beta: Vortex, Elyra (Nv.6+), Vespera (Nv.11+)
- Cidades raidáveis: `ash-keep`, `ruin-citadel`
