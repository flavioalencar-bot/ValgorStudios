# Fase 5 — Auditoria técnica (D030)

**Projeto:** `C:\Valgor_Studio` · **Branch:** `master` · **Base:** `0804fc4`

## Existente (reutilizado)

| Arquivo | Responsabilidade |
|---------|------------------|
| `CityRaidModels.cs` / `CityRaidCatalog` | ash-keep / ruin-citadel + fortificação |
| `CityRaidEncounterService.cs` | Engajar/resolver raid |
| `CityHomeDefenseService.cs` | Defesa legado muralha+Guard |
| `WorldMapSession` | API raid/defesa/Ira/inspeção |
| `DragonBattleRole` + v8 | Support/Assault/Guard |
| UI Torre + HUD mapa | Modos + engajar/resolver |

## Criado / evoluído

| Componente | Papel |
|------------|-------|
| `RaidCombatModels.cs` | Contratos batalha, fortificação, registry |
| `WallCombatResolver` | Durabilidade / Ira / reparo idempotente |
| `CasualtyResolver` | 80/20 + Hospital |
| `WarehouseProtectionResolver` | Proteção / nunca saqueável |
| `RaidLootCalculator` | 20% + capacidade determinística |
| `CityRaidBattleResolver` | Ordem pura de combate |
| `RaidResultApplicationService` | Aplicação atômica / idempotente |
| `LocalDefenseServices` | Scheduler + processor offline |
| Persistência WorldMap | ProcessedOperations + DefenseEvents + wall runtime |
| `Phase5RaidResolverTests` | Unitário + integração A–G + E2E lógica |

## Lacunas pós-entrega (não bloqueantes de lógica)

- Harness Unity visual 1080×640 + rebuild QA Phase5
- UI de reparo de muralha na City
