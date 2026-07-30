# Dragão Fase 4 — Montaria, vínculo com herói e presença no mapa

**Data:** 2026-07-30  
**Base:** `e530b2f` (Fase 3)  
**Persistência:** `valgor.dragons.v7` (migra v6/v5/v4)  
**Build:** `builds/windows/Valgor-QA-Dragon-Phase4/Valgor.exe`

## Fluxo entregue

```text
Selecionar Dragão
→ escolher Herói compatível (Vortex / Elyra / Vespera)
→ criar vínculo de montaria
→ treinar vínculo
→ equipar Herói Montador
→ vincular à marcha PvE
→ visualizar Dragão acompanhando a marcha
→ bônus de vínculo no poder de suporte
→ combate suporte → retorno → persistir
```

## Compatibilidade

| Herói | Min. Nv. dragão |
|-------|-----------------|
| Vortex | 1 |
| Elyra | 6 |
| Vespera | 11 |

## UI Torre

Vínculo Vortex · Treinar montaria · Equipar montaria · Status montaria

## Fora

PvP, ataque a cidades, alianças, cerco, múltiplos dragões, voo manual, combate aéreo livre, monetização.

## Testes

```text
dotnet test tools/Valgor.GameLogic.Tests --filter FullyQualifiedName~DragonFoundation
→ 33 pass / 0 fail
```
