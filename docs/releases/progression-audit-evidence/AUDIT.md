# VALGOR — Auditoria completa de progressão (E2E)

**Data:** 2026-07-29  
**Branch:** `master`  
**Método:** backup de save → mapa de módulos (código) → regressões QA automatizadas → correção P1 Instituto → reteste.

Chat **não** é prova. Evidências: esta pasta + builds QA + reports auto-teste.

---

## 0. Preparação

| Item | Resultado |
|------|-----------|
| Backup save | `save-backups/20260729-181433/playerprefs.reg` (252 chaves, `HKCU\Software\Valgor Studios\Valgor`) |
| Perfil QA City | Slot isolado `city-progression-qa` (`valgor.city.production.v1.city-progression-qa`) — não sobrescreve save jogador |
| Snapshots | Nv.1 (reset QA), Nv.30 + Tier 6 (evolve+reload), construção em andamento (UX auto-test) |

---

## 1. Mapa de módulos (somente existentes)

| Ordem | Módulo | Desbloqueio | Dependências | Estado | Problema |
|---:|---|---|---|---|---|
| 1 | Bootstrap / Loading | Boot | `LoadingFlow` | Completo | Splash rápido / watermark Dev |
| 2 | Main Menu | Pós-loading | Novo Jogo / Continuar | Completo | Card corta botões em ~1080×640 (P1 UI) |
| 3 | Perfil / nome | Novo Jogo | Validação 3–20 | Completo | — |
| 4 | Intro Vortex | Após nome | 4 cards | Completo | Doc `PLAYER_JOURNEY` desatualizado (Beta 0.1) |
| 5 | Tutorial | Pós-city | Passos 0–11 | Parcial | Pouco exercitado em save novo automatizado |
| 6 | Continuar | Perfil+domínio | `lastScene` | Completo | — |
| 7 | City hub + HUD | Pós-intro | Cena City | Completo | Arte silhueta na maioria |
| 8 | Castelo | Seed Ready Nv.1 | Farm+Armazém por alvo; max 30; tiers 1–6 | Completo | — |
| 9 | Fazenda / Serraria / Pedreira / Armazém / Mercado / Torre / Muralha | Seed Ready Nv.1 | Castelo ≥ N (+ cadeia) | Completo | Arte parcial |
| 10 | Mina / Academia / Hospital / Templo / Arena / Lab | Seed Available Nv.0 | Castelo ≥ N (+ deps) | Completo/Parcial | Hospital/Arena/Templo = stubs de fila |
| 11 | Instituto | Locked → Available após Academia Nv.1 | Castelo + Academia | Parcial→corrigido | **P1:** sem unlock até fix `RefreshSoftLocks` |
| 12 | Fila construção 1/1 | Sempre | Construtor único | Completo | — |
| 13 | UX upgrade / Obter mais / auto-refill | Seleção | Wallet + packs | Completo | — |
| 14 | Visual obra (andaimes) | Durante Upgrading | Builder runtime | Completo | Prefabs bake → magenta; runtime OK |
| 15 | Heróis | Nav (sem gate Castelo) | HeroesDemo | Parcial | Heroínas dummy; combate incompleto |
| 16 | Dragões | Nav → Torre | `valgor.dragons.v3` | Parcial | Mesh placeholder; deploy parcial |
| 17 | World Map | Nav (sem gate) | Marcha/energia | Parcial | Arte provisória; criaturas/território parciais |
| 18 | Missões | Nav | `BetaMissions` (8) | Parcial | Capítulo curto |
| 19 | Login online / Alianças / PvP / SvS / Capital | — | Fora da beta | Indisponível | Intencional (D008) |

**Catálogo edifícios (15):** `castle`, `farm`, `lumbermill`, `quarry`, `mine`, `warehouse`, `academy`, `institute`, `hospital`, `market`, `temple`, `dragon-tower`, `arena`, `laboratory`, `wall`.

---

## 2. Sequência principal do jogador — cobertura

| Passo | Como validado | Status |
|-------|---------------|--------|
| Abrir jogo → menu | CheckpointSmoke / builds Beta 0.2.x | OK (smoke) |
| Criar/carregar jogador | Smoke `EnsureLocalProfile` + Continuar | OK em smoke; UI Novo Jogo frágil em 1080×640 |
| Entrar City + recursos | Smoke + HUD QA | OK |
| Castelo Detalhes/Atualizar | BuildingUpgradeUxAutoTest | OK |
| Requisitos → Ir → evoluir deps | UX auto-test + SatisfyRequirement | OK |
| Obter mais / pacotes / auto-refill | UX auto-test | OK |
| Iniciar obra + andaimes + timer | Construction Visual + asserts | OK |
| Acelerar / concluir / liberar construtor | InstantComplete + fila 1/1 | OK |
| Nível + tier suave até 30 | CityProgressionQA Smooth | OK |
| Save → reload → persistência | Auto-tests reload Nv.30 | OK |
| Heróis / Dragões / Mapa / Missões | CheckpointSmoke capturas | Parcial (não é progressão de nível) |

**Limite de método:** cliques sintéticos externos no Input System Unity são instáveis nesta máquina (audit 0.2). Fluxo “como jogador” em UI de menu usa harness interno (`-checkpointSmoke`, `-buildingUpgradeUxTest`, `-cityProgressionQATest`).

---

## 3. Achados (severidade)

| ID | Sev | Achado | Ação |
|----|-----|--------|------|
| A1 | **P1** | Instituto seed `Locked` sem transição para `Available` | **Corrigido:** `CityController.RefreshSoftLocks` (Academia ≥1) |
| A2 | P1 | Menu ~1080×640 corta Confirmar/Sair | Aberto (UI layout); evidência audit 0.2 |
| A3 | P2 | Nav bar pode aparecer no Main Menu com perfil | Aberto (audit 0.2) |
| A4 | P2 | Missões/hospital/arena/templo stubs | Aceito beta |
| A5 | P3 | Watermark Development Build | Aceito builds QA |
| A6 | Info | Prefabs scaffold bake magenta no player | Runtime builder (já em master) |

---

## 4. Regressão

Ver `REGRESSION_RUN.md` nesta pasta.

| Suite | Resultado |
|-------|-----------|
| Construction Visual + Instituto | PASS 17/0 |
| City Progression Smooth → Nv.30 / Tier 6 | PASS |
| Checkpoint smoke Beta 0.2.4 | PASS exit 0 |

**Veredito:** progressão City (construção → tier → max → save) **aprovada** nos harnesses. Instituto P1 corrigido e revalidado. UI Novo Jogo em 1080×640 e stubs (hospital/arena/missões profundas) permanecem abertos como P1/P2 não bloqueantes da progressão de Castelo.
