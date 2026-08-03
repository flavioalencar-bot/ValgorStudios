# Capital Fortress — Sprint 1 — Relatório

**Data:** 2026-08-03  
**Build:** `builds/windows/Valgor-QA-Capital-Sprint1/Valgor.exe`  
**Testes:** 250/0  
**Push:** não

## Arquivos criados

- `client/Assets/Valgor/City/Visual/CityFortressWallConfig.cs`
- `client/Assets/Valgor/City/Visual/CityFortressWallMaterials.cs`
- `client/Assets/Valgor/City/Visual/CityFortressWallModuleFactory.cs`
- `client/Assets/Valgor/City/Visual/CityFortressWallPlacer.cs`
- `client/Assets/_Valgor/Scripts/Editor/CapitalFortressWallKitBuilder.cs`
- `scripts/build-capital-fortress-wall-kit.ps1`
- `scripts/build-qa-capital-sprint1.ps1`
- `tools/Valgor.GameLogic.Tests/CityFortressWallSprint1Tests.cs`
- `docs/releases/capital-wall-sprint1/**`
- Prefabs Modules + Resources material/LOD placeholders (kit builder)

## Arquivos alterados

- `CityEnvironmentBuilder.cs` — `ApplyWallLevel` usa kit Sprint 1 (+ fallback legado)
- `CityProgressionQaNavigateDriver.cs` — screenshots Sprint 1
- `QaCityProgressionWindowsBuild.cs` — `BuildCapitalSprint1Cli`
- `Valgor.GameLogic.Tests.csproj` — link config

## Validação

| Item | Status |
|------|--------|
| Muralha fechada (anel + asas → portão) | OK |
| Portão HF integrado (BuildingId.wall) | OK |
| Castelo / Torre inalterados | OK |
| Save / upgrade / seleção | OK (sem mudanças) |
| Fallback por trecho | OK (factory SafeCreate) |
| LOD0–3 pastas | OK (LOD0 ativo) |

## Performance

| Métrica | Valor |
|---------|-------|
| FPS avg | 166 |
| FPS min | 1,1 (hitch boot) |
| Renderers | ~17358 |
| Tris (est.) | ~533k |
| Mem mono | ~12,8 MB |

## Screenshots

`docs/releases/capital-wall-sprint1/screenshots/01`…`09`

## P0

- Nenhum P0 de gameplay.
- Visual: renderers ainda altos (densidade Art Pass 3 + muralha) — monitorar em Low.

## P1

- Mesh HF reais para segmentos (hoje kit composto + material WallGate).
- LOD1–3 com meshes simplificadas.
- Batching/GPU instancing mais agressivo na muralha.
- Ajuste fino de chord length vs abertura do portão (anti-gap).
