# VALGOR — Próximo Sprint

**Documento:** `docs/project-control/VALGOR_NEXT_SPRINT.md`  
**Base:** consolidação 2026-07-27 (`VALGOR_PRODUCT_MASTER` + `IMPLEMENTATION_STATUS` + `DECISIONS_LOG`)  
**Regra:** sem novas funcionalidades fora desta lista até fechar débitos visuais críticos (D012).  
**Governança:** agente único (D022) — ver `VALGOR_SINGLE_AGENT.md`. Sem trabalho paralelo de segundo agente.

---

## Objetivo do sprint

Fechar o **débito de aderência** da Beta Técnica 0.1: arte mínima nos hubs, clareza documental online vs offline, e estabilidade do executável — **sem** alianças, PvP, SvS, monetização ou mobile store.

---

## Prioridade P0 (obrigatório)

| # | Item | Por quê | Critério de pronto |
|---|------|---------|-------------------|
| 1 | Congelar features de endgame (alianças, guerra, Capital, PvP, SvS, gacha) | D008 / desvio D1 risco | Nenhuma PR nesses épicos |
| 2 | Inventário de placeholders (City / Map / Dragons / heroínas) | Desvio D2 | Lista em `docs/project-control/` ou issue tracker com owner |
| 3 | README: seção **Estado atual da Beta** (offline PlayerPrefs; API não no player) | Desvio D1 | README não promete online no exe beta |
| 4 | Build oficial único: `Valgor-Beta-0.1` | D021 / D3 | Docs e scripts apontam só para Beta-0.1 |
| 5 | Smoke + evidência verdes em build Release (ou doc explícita se Dev for obrigatório) | D011 | Log + exit 0 + PNGs atualizados |

---

## Prioridade P1 (próximo valor de produto)

| # | Item | Nota |
|---|------|------|
| 1 | Arte mínima 1 edifício “herói visual” da cidade (ex.: Castelo ou Torre) | Substituir silhueta sem quebrar UX contextual |
| 2 | 1–2 heroínas com modelo real (não reverter Vortex) | D010 |
| 3 | Dragão: mesh/apresentação acima de placeholder na Torre | Differentiator |
| 4 | World Map: leitura visual de nós (recurso vs criatura vs cidade) | Sem guerra |

---

## Prioridade P2 (plataforma — só após P0)

| # | Item |
|---|------|
| 1 | Plano de sync Client → API (save city/heroes/dragons) |
| 2 | Admin: 1 tela útil (lista heróis) além do stub |
| 3 | Spike Android build (não store) |

---

## Explicitamente FORA deste sprint

- Alianças / ocupação / guerra territorial  
- Capital / Rei do Reino  
- PvP / SvS  
- Monetização / gacha  
- Shooter  
- Menu administrativo central de construções  
- Substituir Vortex por dummy  

---

## Ordem de trabalho sugerida

```text
1. Docs README + freeze escopo
2. Inventário placeholders
3. Build/smoke Release
4. Arte mínima Castelo OU Torre
5. (Opcional) 1 heroína real
```

---

## Definition of Done do sprint

- [ ] Quatro docs de `project-control/` versionados e alinhados entre si  
- [ ] README descreve runtime real da beta  
- [ ] Zero PRs de alianças/PvP/monetização  
- [ ] Executável Beta 0.1 sobe Splash→City→Heróis→Torre→Mapa sem Missing Script / magenta crítico  
- [ ] Lista de placeholders priorizada para o sprint seguinte  

---

## Referências

- `VALGOR_PRODUCT_MASTER.md`  
- `VALGOR_IMPLEMENTATION_STATUS.md` (seção DESVIOS DO PLANO)  
- `VALGOR_DECISIONS_LOG.md`  
- `VALGOR_SINGLE_AGENT.md`  
- `docs/releases/VALGOR_BETA_0_1_CLEANUP_VALIDATION.md`  
- `docs/architecture/city-building-context-ux.md`  
