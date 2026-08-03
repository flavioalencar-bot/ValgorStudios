# 07 — Migração de save

## Etapa A (atual)

`CityCapitalLayoutMigration.TryMigrate` **bloqueada**:

- não altera save;
- não grava `CapitalLayoutVersion = 2`;
- mensagem: aguarda aprovação do preview.

## Contrato Etapa B (após aprovação visual)

| Regra | Obrigatório |
|---|---|
| Detectar V1 | sim |
| Converter só posições / layout version | sim |
| Preservar BuildingId, nível, timers, construção | sim |
| Não alterar recursos / Fases / Dragão / gameplay | sim |
| Idempotente | sim |
| Em falha: manter V1 + log erro | sim |
| Nunca estado parcial | sim |

## Prefs / chave planejada

`valgor.city.capitalLayoutVersion` — **não escrita** na Etapa A.
