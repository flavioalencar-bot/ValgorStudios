# Dragon Tower — correção de mapeamento

O mesh Import_v1 foi **incorretamente** associado a `BuildingId=castle`.

Correção:
- `castle` → exclusivamente `CastleRealVisualLoader` (tiers Resources), sem HF Import_v1
- `dragon-tower` → `VAL_Capital_Architecture_DragonTower` (posição norte Z≈17.6)

BuildingId / Save / seleção / timers / DragonNestView **não** alterados.
