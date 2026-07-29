# Dragão Fase 1 — Ovo e incubação

**Data:** 2026-07-29  
**Escopo:** Castelo ≥ 20 → missão → conquista → incubação + cuidados → nascimento Nv.1  
**Fora:** combate completo, PvP, montaria, múltiplos dragões

## Prova lógica

```text
dotnet test tools/Valgor.GameLogic.Tests --filter FullyQualifiedName~DragonFoundation
→ Aprovado: 20 / 0 falhas
```

Cobertura principal:

| Teste | Critério |
|-------|----------|
| `Seed_StartsLockedEgg_Phase1` | Seed Locked, sem Ready pré-nascido |
| `Castle20_UnlocksEggContent` | Gate Castelo 20 |
| `EggJourney_MissionConquerIncubateCare_BirthsLevel1` | Fluxo completo → Juvenile Nv.1 |
| `Hatch_RequiresCare_DoesNotBirthWithoutIt` | Cuidados obrigatórios |
| `Repository_PersistsJourneyAcrossServiceInstances` | Save fase + nível |

## Fluxo jogável (City)

1. Evoluir Castelo até Nv.20 (QA homologação ou progressão).
2. Abrir **Torre dos Dragões** → **Dragões**.
3. **Aceitar missão do Ovo** → **Buscar o Ovo** → **Iniciar incubação**.
4. **Cuidar do ovo** (≥ 3×, custa comida) até nascer Nv.1.
5. Após nascimento: **Alimentar** (missão beta “Fome do ninho”).

Persistência: `valgor.dragons.v4` (fase, nível, care, castelo sincronizado).

## Referências

- `docs/architecture/dragons.md`
- `VALGOR_DECISIONS_LOG.md` → D025
- `VALGOR_NEXT_SPRINT.md`
