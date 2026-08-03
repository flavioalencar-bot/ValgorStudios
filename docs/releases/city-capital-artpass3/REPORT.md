# Art Pass 3.0 — Relatório de validação

**Data:** 2026-08-03  
**Build:** `builds/windows/Valgor-QA-Capital-ArtPass3/Valgor.exe`  
**Testes:** 247/0 (`Valgor.GameLogic.Tests`)  
**Push:** não realizado

## Assets substituídos / melhorados (camada visual)

| Área | Antes | Depois |
|------|-------|--------|
| Muralha ambiente | Anel aberto + portão procedural duplicado | Anel 28 segs fechado (abertura sul p/ HF) + 4 cantos + asas → WallGate HF |
| Habitação densify | Cubos coloridos (rings A–E) | Habitação vernacular multi-peça (plinto/corpo/telhado/chaminé) |
| UrbanDensity | Anéis cottage coloridos | Lean (jardins/lampiões) — casas via vernacular |
| Pads de distrito | Tapetes contrastantes | Solo/gramado suavizado |
| Vegetação Nature | Copa esfera única | Camadas (3) / pinheiro cônico |
| Anéis de floresta | Esfera única | Camadas / pinheiros |
| Distrito Real | Pad básico | Escadaria, nobres, sebes, urna |
| Distrito Dragões | Platô prévio | Rochas, pinheiros, caminho/degraus |
| Econômico | Props leves | Barracas, depósito, carroça, barris, pátio |
| Militar | Props leves | Pátio, estandartes, racks, vigia, tendas |
| Estradas | Flat | Calçadas + cruzamentos + approach |
| Água | Flat | Quedas, pedras, juncos, rails |
| Atmosfera | 9 NPCs wander | +12 marcadores estáticos |
| Telhados oficiais | RoofRed/Blue vivos | Terracota / ardósia (ArtPass3) |

## Assets HF (pipeline / loaders)

- `wall` → `VAL_Capital_Architecture_WallGate`
- `dragon-tower` → `VAL_Capital_Architecture_DragonTower` + platô
- `castle` → `Castle_Tier*` via `CastleRealVisualLoader`

## Ainda em blockout (sem mesh HF externo)

- Segmentos de muralha (primitivas pedra, sem kit modular FBX)
- Habitação / props (primitivas)
- Vegetação (primitivas camadas)
- Estradas / água flat
- NPCs cápsula
- Horizonte (esferas)
- 14 BuildingIds sem prefab HF no catálogo (proxies procedurais)

## Performance (runtime QA)

Fonte: `profiler-runtime.md`

| Métrica | Valor |
|---------|-------|
| FPS avg | 314 |
| FPS min | 0,9 (hitch boot) |
| Renderers | 2278 |
| Triângulos (est.) | ~294k |
| Mem mono | ~10,6 MB |

## Screenshots

`docs/releases/city-capital-artpass3/screenshots/` — `01`…`11`

## P0

- Nenhum P0 de gameplay (IDs/Save/seleção/Fases 1–5 intactos).
- Visual: portão HF ainda pode mostrar massa cinza residual do proxy se ArtHF não cobrir 100% do volume — monitorar close-up.

## P1

- Importar pack HF de segmentos de muralha / casas / vegetação para substituir primitivas.
- LOD / culling mais agressivo (2278 renderers).
- Reduzir hitch de FPS min no boot (warmup/batch).
- Preencher catálogo dos 14 edifícios restantes com prefabs HF quando disponíveis.

## Restrições respeitadas

Não moveu Castelo / Torre / muralha lógica. Sem novos BuildingIds. Sem Save. Sem mudança de seleção. Sem lógica Fases 1–5.
