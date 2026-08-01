# 10 — Migração de save 15 → 17

Camada única: `BuildingIdMigration`.

## Regra

1. Ao carregar snapshot (`CityEconomy` / `LocalProductionRepository`), aplicar `MigrateSnapshot`.
2. Remapear chaves:
   - `institute` → `hero-hall` (nível/estado/timer preservados)
   - `laboratory` → `training-ground`
   - `sawmill` → `lumbermill`
3. Em conflito, manter maior nível / estado mais avançado.
4. Inicializar ausentes:
   - `residence` → Available, Nv.0
   - `foundry` → Available, Nv.0
   - `hero-hall` → Locked, Nv.0 (se não veio de institute)
   - `training-ground` → Available, Nv.0 (se não veio de laboratory)
5. Carteira e demais edifícios intactos.
6. Persistência subsequente grava apenas IDs canônicos.

## Validação

- Testes: `CityBuildingMigrationTests` (migração 15→17 + idempotência) verdes.
- Wipe Nova Jornada limpa 17 canônicos + aliases.

## Não faz

- Elevar Residência/Fundição ao nível do Castelo automaticamente.
- Apagar progresso de institute/laboratory.
- Duplicar entidades lógicas.
