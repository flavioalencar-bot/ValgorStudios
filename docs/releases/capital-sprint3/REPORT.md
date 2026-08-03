# Capital Sprint 3 — Layer Architecture — Relatório

**Data:** 2026-08-03  
**Commit:** `feat(city): replace capital architecture placeholders` (`546fa43`)  
**Build:** `builds/windows/Valgor-QA-Capital-Sprint3/Valgor.exe`  
**Testes unitários:** 256/0  
**AutoTest player:** OK (tier swap smooth)  
**Push:** não

## Objetivo

Eliminar o aspecto de blockout da Capital: os 17 edifícios oficiais passam a ter arquitetura definitiva (HF externo) ou **proxy de alta fidelidade** (procedural + materiais do pipeline + dressing temático), nas mesmas posições.

## Arquivos alterados (principais)

| Arquivo | Papel |
|---------|--------|
| `CityArchitectureSprint3Config.cs` | Flag / lista dos 17 / stages |
| `CityOfficialBuildingDressing.cs` | Fundação, escadas, jardins + props por BuildingId |
| `CityBuildingMeshFactory.cs` | Proxies enriquecidos + Attach dressing |
| `CityScenarioCompositionBuilder.cs` | Sem anéis de casas coloridas |
| `CityUrbanDensityBuilder.cs` | Densidade lean (sem cottages cubo) |
| `CityArtPass3Config.cs` | HF list expandida; vernacular fora do RemainingBlockout |
| `CityLayout.cs` | Identidade cromática neutra (Sprint 3) |
| `CityArtPass2PerfProbe.cs` | Profiler → `docs/releases/capital-sprint3/` |
| `CityProgressionQaNavigateDriver.cs` | Screenshots 01–13 |
| `QaCityProgressionWindowsBuild.cs` | `BuildCapitalSprint3Cli` |
| `scripts/build-qa-capital-sprint3.ps1` | Build Windows QA |
| `CityArchitectureSprint3Tests.cs` | Suite de aceite config/âncoras |

## Assets / pipeline

| BuildingId | Fonte visual |
|------------|--------------|
| castle | `CastleRealVisualLoader` + CastleArtDressing |
| wall | Catálogo HF WallGate + kit muralha Sprint 1 |
| dragon-tower | Proxy + DragonTowerArtDressing |
| market…mine (14) | Proxy MeshFactory + `ArchSprint3Dressing` (tendas, forjas, etc.) |

Fallback procedural permanece se HF/catálogo falhar. Sem novos BuildingIds.

## Dressing temático (amostra)

- Mercado: tendas, barracas, caixas, barris, carroça, toldo  
- Armazém: depósitos, pallets, sacarias, madeira  
- Fundição: chaminés, fornalha, carvão, bigorna, fumaça preparada  
- Academia: ala biblioteca, cúpula/observatório, bandeiras  
- Hospital: enfermaria, jardim, tenda, poço, cruzamentos  
- Arena / Campo: arquibancadas, alvos, bonecos, tendas  
- Salão / Templo: colunas, escadaria, brasões, altar, luz  
- Fazenda / Serraria / Pedreira / Mina: plantações, toras, guindaste, túnel, lanternas  
- Residência: mansão pedra/ardósia (sem casas coloridas)

## Build

```
builds/windows/Valgor-QA-Capital-Sprint3/Valgor.exe
scripts/build-qa-capital-sprint3.ps1
```

## Performance (runtime QA)

| Métrica | Valor |
|---------|-------|
| FPS avg | 152,9 |
| FPS min | 1,8 (hitch boot) |
| Renderers | ~17356 |
| Tris (est.) | ~556k |
| Mem mono | ~14,3 MB |

Fonte: `profiler-runtime.md`

## Screenshots

`docs/releases/capital-sprint3/screenshots/`

01-overview · 02-castelo · 03-mercado · 04-hospital · 05-arena · 06-templo · 07-herohall · 08-fundicao · 09-fazenda · 10-mina · 11-pedreira · 12-1080x640 · 13-1920x1080

## Validação

| Critério | Status |
|----------|--------|
| Suite unitária | 256/0 |
| AutoTest player (tier swap) | OK |
| Build QA Sprint 3 | OK |
| Gameplay / Save / BuildingId | Intactos |
| Seleção / Upgrade | Intactos (não tocados) |
| Casas coloridas de blockout | Desligadas |
| Placeholder cubo “puro” nos 17 slots | Substituído por proxy + dressing |

## P0

Nenhum.

## P1

- Proxies dos 14 slots sem mesh HF externa ainda usam primitivas compostas (não assets de arte importados). Troca por prefabs do `CityArtAssetCatalog` quando o pack de arquitetura chegar.
- Horizonte / vegetação ambiental ainda com volumes procedurais (fora do escopo dos 17 edifícios).
