# Capital — Art Pass 3.0 (ambiente completo)

**Data:** 2026-08-03  
**Build:** `builds/windows/Valgor-QA-Capital-ArtPass3/Valgor.exe`  
**Escopo:** exclusivamente visual (sem BuildingId / Save / seleção / Fases 1–5)

## Objetivo

Tratar a Capital como ambiente completo: fechar a muralha, reduzir blockout colorido e densificar distritos com o pipeline existente.

## Entregas por etapa

| # | Etapa | Entrega |
|---|-------|---------|
| 1 | Muralha fechada | Anel 28 segs + cantos + asas → Portão HF |
| 2 | Blockout | Habitação vernacular (pedra/madeira/telha sóbria) |
| 3 | Distrito Real | Escadaria, residências nobres, sebes, urna |
| 4 | Distrito Dragões | Rochas, pinheiros, caminho de acesso |
| 5 | Econômico | Barracas, depósitos, carroça, barris, pátio |
| 6 | Militar | Pátio, estandartes, racks, torre de vigia, tendas |
| 7 | Vegetação | Copas em camadas / pinheiros cônicos |
| 8 | Estradas | Calçadas + cruzamentos + approach do portão |
| 9 | Água | Quedas, pedras de margem, juncos, rails de ponte |
| 10 | Atmosfera | 12 marcadores NPC estáticos |

## Assets HF (catálogo / loaders)

- `wall` → WallGate  
- `dragon-tower` → DragonTower + platô  
- `castle` → Castle_Tier* (loader existente)

## Ainda blockout (sem mesh HF externo)

habitação vernacular · vegetação por primitivas · estradas flat · água flat · props de distrito · NPCs cápsula · horizonte esferas

## Não alterado

IDs · posições lógicas Castelo/Torre/Muralha · Save · seleção · Fases 1–5 · `DragonNestView`

## Evidências

Ver `screenshots/` e `REPORT.md`.
