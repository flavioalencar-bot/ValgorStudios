# 06 — Resultados de testes

## Comando

```powershell
dotnet test tools/Valgor.GameLogic.Tests
```

## Resultado (2026-08-01)

```text
Total: 201
Aprovados: 201
Falhas: 0
```

| Suite | Contagem | Resultado |
|-------|----------|-----------|
| Completa | 201/0 | OK (≥186) |
| CityCapitalLayoutTests | 6/0 | OK |
| CityOfficialBuildingsTests | 5/0 | OK |
| CityBuildingMigrationTests | 2/0 | OK |
| CitySelectionTests | 7/0 | OK |
| Phase5RaidResolverTests | 16 | OK |
| DragonFoundationTests (incl. Phase5) | 35 | OK |
| Phase5 (Raid + Dragon Phase5) | 18 | OK |

Build QA: `scripts/build-qa-city-capital.ps1` → OK  
`Valgor-QA-Capital-Blueprint/Valgor.exe` gerado.
