# 09 — Auditoria visual no player

## Ambiente

- Build: `builds/windows/Valgor-QA-Capital-Blueprint/Valgor.exe`
- Resolução: **1080×640** (também exercitado pelo smoke)
- Flags: `-checkpointSmoke -captureEvidence`

## Checklist

| # | Item | Resultado |
|---|------|-----------|
| 1 | 17 edifícios no catálogo/layout | OK (testes + seeds) |
| 2 | Selecionáveis | OK (smoke percorre os 17 relevantes) |
| 3 | Painel correto | OK (ex.: Residência Nv.0 Disponível) |
| 4 | Muralha/portão → `wall` | OK (log: clique proxy → wall) |
| 5 | Residência ≠ decoração | OK (painel “Residência oficial”) |
| 6 | Fundição ≠ decor industrial | OK (label “Fundição”) |
| 7 | Campo ≠ Academia/Arena | OK (label “Campo de Treinamento”) |
| 8 | Salão ≠ Castelo/Academia | OK (ID hero-hall + painel) |
| 9 | Pan/zoom | OK (smoke/câmera City) |
| 10 | HUD | OK (nav inferior; recursos no topo QA) |
| 11 | Tiers I / III / VI | OK (`city-castle-tier1/3/6`) |
| 12 | Sobreposição crítica | Nenhuma P0/P1 |

## Screenshots

Pasta `screenshots/` — arquivos `01`–`16` gerados.
