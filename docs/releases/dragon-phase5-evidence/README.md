# Dragão Fase 5 — Raid PvE NPC + defesa local (D030)

**Data:** 2026-07-31  
**Base homologada F1–4:** `0804fc4`  
**Branch:** `master` · `C:\Valgor_Studio`  
**Persistência dragão:** `valgor.dragons.v8` (migra v7→v8)  
**Decisão:** D030 — raid **PvE** a cidades NPC + defesa local simulada (**sem** PvP multiplayer)

## Fluxo ofensivo

```text
inspecionar cidade NPC → formação → Support|Assault (+ Ira Nv.21)
→ marcha → engajar → resolver (Muralha → defensores → Hospital → Armazém → saque)
→ relatório → persistir (idempotente por BattleId/LootTransactionId)
```

## Fluxo defensivo

```text
DefenseEvent pendente → login/tick → Guard se válido → Muralha/Hospital/Armazém
→ relatório → Completed · reload não duplica
```

## Alvos

| Nó | Defesa | Muralha | Hospital | Raidável |
|----|--------|---------|----------|----------|
| ash-keep | 110 | Nv.10 / 5000 | 3000 | sim |
| ruin-citadel | 160 | Nv.14 / 8000 | 4500 | sim |

## Componentes

- Resolvers puros: `WallCombatResolver`, `CasualtyResolver`, `WarehouseProtectionResolver`, `RaidLootCalculator`, `CityRaidBattleResolver`
- Aplicação: `RaidResultApplicationService` + `ProcessedOperationRegistry`
- Defesa offline: `LocalDefenseScheduler` / `PendingDefenseProcessor`
- UI: inspeção/preparação no HUD do mapa + Ira do Dominador + modos na Torre

## Testes (lógica)

```text
dotnet test tools/Valgor.GameLogic.Tests --filter FullyQualifiedName~DragonFoundation
→ 35 pass / 0 fail

dotnet test tools/Valgor.GameLogic.Tests --filter FullyQualifiedName~Phase5
→ 18 pass / 0 fail

dotnet test tools/Valgor.GameLogic.Tests
→ 180 pass / 0 fail
```

## Fora de escopo

PvP multiplayer, alianças/SvS, cerco multi-turno, múltiplos dragões, monetização.

## Pendências conhecidas

- Build Unity QA Phase5 + harness visual 1080×640: executar `scripts/build-qa-dragon-phase5.ps1` quando o Editor estiver livre.
- Reparo de muralha na City (job) coberto em resolver unitário; UI de reparo da City pode polir depois.
