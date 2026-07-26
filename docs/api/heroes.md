# API de Heróis

Fonte de verdade: `docs/game-design/heroes/`.

## Endpoints

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| GET | `/api/heroes/catalog` | anônimo | Catálogo completo (11 heróis) |
| GET | `/api/heroes/{heroId}` | anônimo | Detalhe por ID interno |
| GET | `/api/heroes/factions` | anônimo | Facções + multiplicador de vantagem |
| GET | `/api/heroes/team-bonuses` | anônimo | Bônus 3 / 3+2 / 4 / 5 |
| GET | `/api/players/me/heroes` | JWT | Roster do jogador |
| POST | `/api/teams/validate` | anônimo | Valida equipe e calcula bônus |
| POST | `/api/battle/{battleId}/heroes/{heroId}/special/activate` | body `playerId` | Ativa poder especial (autoritativo) |

## Ativação de especial

```json
POST /api/battle/{battleId}/heroes/{heroId}/special/activate
{
  "playerId": "00000000-0000-0000-0000-000000000000",
  "idempotencyKey": "unique-key"
}
```

Estados: `READY` → `ACTIVE` → `COOLDOWN` → `READY`. Duração e recarga vêm do seed.
