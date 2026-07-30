# Dragons Foundation

Sistema de dragões em `Assets/Valgor/Dragons`.

## Objetivo

**Fase 1:** Castelo Nv.20 → ovo → incubação → nascimento Nv.1.  
**Fase 2:** progressão Nv.1→30 (XP, alimentação, vínculo, energia, saúde, caps, rituais, timers, aceleradores).  
**Fase 3:** habilidades (3 slots) + combate PvE como suporte automático.

## Limite

Integração via `IDragonGateway`. Sem PvP, montaria completa, múltiplos dragões ou controle manual em batalha.

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
Estágios visuais: Ovo → Filhote → Jovem → Adolescente → Adulto jovem → Adulto → Ancestral
```

## Combate PvE (Fase 3)

```text
READY (energia/saúde OK) → DEPLOYED (marcha)
→ engajar criatura → suporte automático (habilidades)
→ resolver (poder heróis + suporte dragão)
→ gastar energia / dano em saúde → recall → Recovering/Injured
```

| API | Papel |
|-----|--------|
| `TrySetAbilitySlot` | Configura loadout (0–2) |
| `DescribeDragonAbilities` | Texto HUD |
| `TryEnterCombatForMarch` | Valida energia/saúde no engage |
| `TryApplyCombatOutcomeForMarch` | Aplica custo/dano/XP/ferida |
| `GetSupportPowerForMarch` | Poder com multiplicadores de habilidade |

Persistência: `valgor.dragons.v6` (migra `v5`/`v4`).

## Seed

- `dragon-ember-1` (`ember-whelp`) — LOCKED até a jornada; após nascimento, progressão + combate.
