# Dragão Fase 2 — Evolução Nv.1→30

**Data:** 2026-07-29  
**Base:** `3dbf47a` (Fase 1) · Persistência `valgor.dragons.v5` (migra v4)  
**Build:** `builds/windows/Valgor-QA-Dragon-Phase2/Valgor.exe`

## Escopo entregue

| Item | Status |
|------|--------|
| Nível 1→30 + XP | OK |
| Alimentação → XP / energia / saúde / vínculo | OK |
| Caps Castelo + Torre | OK |
| Requisitos (XP, energia, saúde, recursos) | OK |
| Timers level-up + ritual | OK |
| Acelerador (diamantes) | OK |
| Rituais 6 / 11 / 16 / 21 / 26 | OK |
| Estágios por nível | OK |
| Save + migração v4→v5 | OK |

## Caps

- Castelo: `max = min(30, castleLevel)` (castle ≥ 20)
- Torre: `max = min(30, 5 + (towerLevel-1)*2)`
- Efetivo: `min(castelo, torre, 30)`

## Testes

```text
dotnet test tools/Valgor.GameLogic.Tests --filter FullyQualifiedName~DragonFoundation
→ 26 pass / 0 fail
```

## P0 / P1

- **P0:** progressão Nv.1→30 jogável na Torre (alimentar, evoluir, ritual, acelerar); save v5.
- **P1:** polish visual por estágio; auto-teste Unity E2E até Nv.30 (não bloqueante).

## Limitações restantes

- Sem combate completo / PvP / montaria / múltiplos dragões.
- Mesh do dragão permanece placeholder.
- Estágio visual 3D ainda não troca por ritual (só dados + UI texto).
- QA build usa define de progressão City; smoke Fase 2 via testes + Torre manual.
