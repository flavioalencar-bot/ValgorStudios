# VALGOR — Bug list Beta 0.2 (auditoria executável)

**Data:** 2026-07-27 · Commit `f732589` · Exe `Valgor-Beta-0.2`  
**Regra desta etapa:** apenas registro — **sem correção**.

Severidade: **P0** bloqueia jornada/dados · **P1** grave · **P2** médio/provisório · **P3** menor.

---

## Resumo

| Severidade | Quantidade |
|------------|------------|
| P0 | **3** |
| P1 | **6** |
| P2 | **8** |
| P3 | **4** |
| **Total** | **21** |

---

## P0

| ID | Título | Evidência |
|----|--------|-----------|
| B0.2-001 | **World Map falha ao carregar energia** — `FormatException` em `EnergyPersistenceRepository.LoadFromPrefs` (DateTime) | `smoke-audit.log` |
| B0.2-002 | **World Map viewport preto** após falha de bootstrap (`NullReferenceException` em `WorldMapSceneHost`) — só barra inferior | `04-worldmap.png`, `05`, `06`, `vis-12` (19:23, ~63 KB) |
| B0.2-003 | **Novo Jogo / Confirmar cortados** em janela ~1080×640 — card colado à direita/baixo impede concluir nome | `player-audit/04-name-typed.png`, `r4-*` |

## P1

| ID | Título | Evidência |
|----|--------|-----------|
| B0.2-010 | **Barra Cidade/Heróis/… no Menu Principal** (com e sem perfil) | `00-main-menu.png` |
| B0.2-011 | **PlayerPrefs Editor ≠ Player** — save da City no Editor (`flavio`) não aparece no exe | Registry Editor vs `Valgor Studios\Valgor` |
| B0.2-012 | **Missões** só toast “em breve” — botão engana jornada | `BetaNavigationBar` + nav em todas as telas |
| B0.2-013 | World Map (quando legível) expõe **códigos técnicos** (`tide-crab`, `ash-drake`) | `vertical-slice/05-worldmap-node.png` |
| B0.2-014 | Cards de heróis ainda usam **títulos** (“A Consorte…”, “A Maga…”) em vez de nomes curtos em vários slots | `02-heroes-vortex.png` |
| B0.2-015 | Instabilidade World Map: mesma build OK às 18:47 e preta às 19:23 | comparar `vertical-slice/04` vs `audits/.../04` |

## P2

| ID | Título | Evidência |
|----|--------|-----------|
| B0.2-020 | Arte City silhueta / low-poly; chão plano | `art-01-city-full.png` |
| B0.2-021 | Muralha visual em **segmentos desconexos** (função OK) | `ux-31*`, `ux-32` |
| B0.2-022 | Retratos de heróis = iniciais; preview Vortex = blocos | `vis-10-heroes.png` |
| B0.2-023 | Watermark **Development Build** em todas as telas | várias |
| B0.2-024 | **Instituto** no catálogo/save sem evidência UX no smoke | catálogo + ausência `ux-*institute*` |
| B0.2-025 | Splash/Loading não capturados como etapas distintas | sessão player-audit |
| B0.2-026 | Doc jornada ainda **Beta 0.1** / path Checkpoint | `PLAYER_JOURNEY_BETA_0_1.md` |
| B0.2-027 | Retorno City: HUD superior às vezes ausente no frame | `07-city-return.png` |

## P3

| ID | Título | Evidência |
|----|--------|-----------|
| B0.2-030 | Versão duplicada no menu (“Valgor — Beta 0.2” + “Beta 0.2 · offline”) | `00-main-menu.png` |
| B0.2-031 | Intro: quatro cards com título “Vortex” repetido | `MainMenuController` |
| B0.2-032 | Comentários/código ainda citam “Beta 0.1” em vários arquivos | ex. `LocalPlayerProfile` |
| B0.2-033 | Fundo do menu preto sem arte de brand | `00-main-menu.png` |

---

## Não bugs (comportamento esperado / OK)

- Upgrade bloqueado por pré-requisito + botão **Ir**.
- **Concluir Agora** desabilitado fora de upgrade ativo.
- Continuar oculto sem perfil.
- Wipe Novo Jogo (quando alcançável) separado do Continuar.
- Vortex presente na lista (não substituído por dummy).
- Muralha selecionável (`Building clicked: wall` + proxy).
- Alimentar dragão executado no smoke (`DebugFeedDragon`).
- Save Player persiste name/lastScene/city/dragons/worldmap após quit.

---

## Prioridade sugerida (pós-auditoria)

1. P0 energia/DateTime + estabilidade World Map  
2. P0 layout menu / Novo Jogo  
3. P1 nav no menu + dual-save Editor/Player (doc ou unificação)  
4. P1 Missões (implementar ou esconder)  
5. P2 arte / Instituto / docs
