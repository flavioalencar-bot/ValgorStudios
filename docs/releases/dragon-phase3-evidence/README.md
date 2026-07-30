# Dragão Fase 3 — Habilidades e combate PvE

**Data:** 2026-07-30  
**Base:** `8e5b04f` (Fase 2 P1)  
**Persistência:** `valgor.dragons.v6` (migra v5/v4)  
**Build:** `builds/windows/Valgor-QA-Dragon-Phase3/Valgor.exe`

## Fluxo entregue

```text
Dragão elegível (energia/saúde)
→ configurar habilidades (Torre)
→ vincular à marcha (formação)
→ engajar criatura PvE
→ suporte automático (sem controle manual)
→ resolver combate (poder heróis + dragão)
→ consumir energia / aplicar dano
→ recompensas da criatura
→ persistir
→ recall + recuperação (Injured se ferido)
```

## Habilidades

| Id | Nome | Unlock | Efeito |
|----|------|--------|--------|
| ember-breath | Sopro de Brasa | 1 | +15% poder |
| scale-guard | Escama Protetora | 6 | −25% dano recebido |
| bond-roar | Rugido do Vínculo | 11 | +10% + vínculo |
| ash-surge | Surto de Cinzas | 16 | +25% poder, +energia |
| ancestral-aegis | Égide Ancestral | 26 | −40% dano + cura |

Slots: 3. UI Torre: **Habilidades** / **Equipar próxima habilidade** (loadout recomendado).

## Fora desta fase

PvP, ataque a cidades, alianças, montaria visual completa, múltiplos dragões, reprodução, morte permanente, monetização, controle manual em batalha.

## Testes

```text
dotnet test tools/Valgor.GameLogic.Tests --filter FullyQualifiedName~DragonFoundation
→ 30 pass / 0 fail
```
