# VALGOR — Matriz de Implementação e Auditoria de Aderência

**Documento:** `docs/project-control/VALGOR_IMPLEMENTATION_STATUS.md`  
**Auditoria:** 2026-07-27  
**Base:** código + docs + commits + builds — **não** chat.

### Legenda Situação

| Valor | Significado |
|-------|-------------|
| NÃO INICIADO | Sem código relevante |
| EM DESENVOLVIMENTO | Código parcial |
| LÓGICA PRONTA | Domínio/simulação OK; falta UI/arte/exe |
| INTEGRADO | Ligado ao fluxo do jogador no código |
| VISÍVEL NO EXECUTÁVEL | Confirmado em build Windows / smoke / evidência |
| VALIDADO PELO USUÁRIO | Aceite humano documentado |
| BLOQUEADO | Impedido por débito técnico/visual |
| FORA DO PLANO | Existe mas não pertence ao escopo atual / desvio |

Colunas Planejada / Código / Integrada / UI / Arte / Executável / Validada: **S** = sim, **P** = parcial, **N** = não.

---

## Matriz

| Epic | Funcionalidade | Planejada | Código | Integrada | UI | Arte | Executável | Validada pelo usuário | Situação | Evidência | Bloqueio |
|------|----------------|:---------:|:------:|:---------:|:--:|:----:|:----------:|:---------------------:|----------|-----------|----------|
| Game Core | Bootstrap / GameSession / SceneFlow | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | `GameCore`, `PLAYER_JOURNEY_BETA_0_1.md`, smoke exit 0 | — |
| Game Core | Save / Load PlayerPrefs | S | S | S | S | N/A | S | P | VISÍVEL NO EXECUTÁVEL | `valgor.player.v1`, Continuar | — |
| Splash | Brand / Loading splash | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | LoadingScene + Bootstrap | Watermark Dev Build |
| Loading | Barra / progresso fake→real | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | LoadingSceneController | — |
| Main Menu | Novo Jogo / Continuar | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | MainMenuScene | — |
| Main Menu | Login online | S | P | N | N | N | N | N | LÓGICA PRONTA | API Auth no server; client offline | Integração API |
| Novo Jogo | Reset + intro Vortex | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | JourneyIntro / tutorial | — |
| Continue | Resume última tela | S | S | S | S | N/A | S | P | VISÍVEL NO EXECUTÁVEL | SaveSlot | — |
| City | Cena cidade + HUD | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | CityScene, smoke | Arte silhueta |
| City | Castelo | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | BuildingCatalog | Placeholder visual |
| City | Fazenda | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | Produção food | Placeholder |
| City | Serraria | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | wood | Placeholder |
| City | Pedreira | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | stone | Placeholder |
| City | Mina | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | ore/gold | Placeholder |
| City | Armazém | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | capacity | Placeholder |
| City | Academia | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | research slot | Placeholder |
| City | Instituto | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | research | Placeholder |
| City | Hospital | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | heal queue | Placeholder |
| City | Mercado | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | trade stub UI | Placeholder |
| City | Templo | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | buffs | Placeholder |
| City | Torre dos Dragões | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | dragons.md | Placeholder 3D |
| City | Arena | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | train | Placeholder |
| City | Laboratório | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | research | Placeholder |
| City | Produção de recursos | S | S | S | S | N/A | S | P | VISÍVEL NO EXECUTÁVEL | CityEconomy | — |
| City | Coleta no prédio | S | S | S | S | N/A | S | P | VISÍVEL NO EXECUTÁVEL | Collect action | — |
| City | Upgrade contextual | S | S | S | S | N/A | S | P | VISÍVEL NO EXECUTÁVEL | BuildingContextMenu | — |
| City | Filas (build/train/research) | S | S | S | S | N/A | S | P | VISÍVEL NO EXECUTÁVEL | CityQueues | — |
| City | Menu admin central construções | N | N | N | N | N | N | N | FORA DO PLANO | Proibido D017 | — |
| Heroes | Fundação módulo / catálogo | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | HeroesDemo | — |
| Heroes | Vortex modelo real | S | S | S | S | S | S | P | VISÍVEL NO EXECUTÁVEL | production/Vortex + preview | — |
| Heroes | 10 heroínas | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | Seed + dummies | Arte real pendente |
| Heroes | Facções (3) | S | S | P | P | N | P | N | INTEGRADO | Data + labels | UI profunda |
| Heroes | Poderes duração/CD | S | S | P | P | N | P | N | LÓGICA PRONTA | SpecialPower + API | Batalha full |
| Heroes | Skins | S | P | N | N | N | N | N | EM DESENVOLVIMENTO | Campos data | Pipeline arte |
| Heroes | Formação | S | P | N | P | N | N | N | EM DESENVOLVIMENTO | UI parcial | — |
| Heroes | Combate completo | S | P | N | N | N | N | N | EM DESENVOLVIMENTO | Simulação especial | — |
| Dragons | FSM ovo→adulto | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | dragons.v3 / Torre | Placeholder mesh |
| Dragons | Alimentação / fome / stamina | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | DragonsController | — |
| Dragons | Vínculo / evolução / recover | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | architecture/dragons.md | — |
| Dragons | Deploy / combate | S | P | P | P | N | P | N | EM DESENVOLVIMENTO | Deploy flags | Combate mapa |
| Dragons | Presença visual no mapa | S | P | P | P | N | P | N | EM DESENVOLVIMENTO | Nodes / markers | Arte |
| World Map | Cena + câmera + nós | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | WorldMapScene | Placeholder |
| World Map | Regiões / territórios overlay | S | S | P | P | N | P | N | INTEGRADO | Territory overlay | Arte / regras |
| World Map | Recursos / gather | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | March gather | — |
| World Map | Criaturas | S | S | P | P | N | P | N | INTEGRADO | Creature nodes | Combate |
| World Map | Marchas | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | March system | — |
| World Map | Cidades / vilarejos | S | S | P | P | N | P | N | INTEGRADO | Node types | Ocupação |
| World Map | Alianças | S | N | N | N | N | N | N | NÃO INICIADO | Escopo pós-beta D008 | — |
| World Map | Guerra territorial | S | N | N | N | N | N | N | NÃO INICIADO | Pós-beta | — |
| World Map | Capital / Rei do Reino | S | N | N | N | N | N | N | NÃO INICIADO | Endgame | — |
| PvP | Combate jogador×jogador | S | N | N | N | N | N | N | NÃO INICIADO | D008 | — |
| SvS | Server vs Server | S | N | N | N | N | N | N | NÃO INICIADO | D008 | — |
| Monetização | Loja / IAP / gacha | S | N | N | N | N | N | N | NÃO INICIADO / FORA beta | D008 gacha | — |
| Backend | Auth JWT | S | S | N* | N | N/A | N* | N | LÓGICA PRONTA | server Auth | *não no player beta |
| Backend | Heroes API | S | S | N* | N | N/A | N* | N | LÓGICA PRONTA | 23 testes server | Offline client |
| Backend | City / Dragons / Map API | S | N | N | N | N | N | N | NÃO INICIADO | — | Persistência só local |
| Admin | Login + Dashboard | S | S | P | P | N | N | N | EM DESENVOLVIMENTO | admin React stub | Sem deploy prod |
| Infra | Docker Compose / Postgres / Redis | S | S | P | N/A | N/A | N | N | EM DESENVOLVIMENTO | compose files | Workers stub |
| Infra | Nginx SSL produção | S | P | N | N/A | N/A | N | N | EM DESENVOLVIMENTO | SSL-SETUP pendente (regra DirectPaper; Valgor TBD) | — |
| Android | Build / store | S | N | N | N | N | N | N | NÃO INICIADO | Alvo produto | Player Windows only |
| iOS | Build / store | S | N | N | N | N | N | N | NÃO INICIADO | Alvo produto | — |
| Builds | Windows Beta 0.1 | S | S | S | S | P | S | P | VISÍVEL NO EXECUTÁVEL | `Valgor-Beta-0.1\Valgor.exe` | Shader strip / Dev Build |
| Arte refs | design-references imagens | S | N | N | N | N | N | N | NÃO INICIADO | INDEX + LAST_Z map; **0 imagens** | Assets faltando |

\* Backend existe e testa no monorepo; **não** integrado ao executável beta.

---

## Auditoria de aderência por módulo

Para cada módulo: (1) alinhado? (2) no fluxo do jogador? (3) no executável? (4) placeholder? (5) duplicado? (6) cedo demais? (7) desvio? (8) ação?

### Game Core / Splash / Loading / Main Menu / Save

| # | Resposta |
|---|----------|
| 1 | Sim — jornada documentada |
| 2 | Sim |
| 3 | Sim |
| 4 | Arte brand parcial |
| 5 | Não (pasta Checkpoint vs Beta-0.1 = legado/espelho, não lógica duplicada) |
| 6 | Não |
| 7 | Não |
| 8 | **Continuar** — polir arte splash; remover watermark Dev quando release |

### City (edifícios, produção, coleta, upgrade, filas)

| # | Resposta |
|---|----------|
| 1 | Sim — UX contextual alinhada a D017 |
| 2 | Sim — hub principal |
| 3 | Sim |
| 4 | **Sim** — silhuetas 3D |
| 5 | Não |
| 6 | Não |
| 7 | Não |
| 8 | **Continuar** arte; **não** adicionar menu central |

### Heroes / Vortex / heroínas / poderes

| # | Resposta |
|---|----------|
| 1 | Sim |
| 2 | Sim (HeroesDemo) |
| 3 | Sim |
| 4 | Heroínas = dummy; Vortex = real |
| 5 | Não duplicar lógica fora de `Assets/Valgor/Heroes/**` (mesmo com agente único — D022) |
| 6 | Powers API sem batalha full = aceitável fundação |
| 7 | Não |
| 8 | **Continuar** pipeline arte heroínas; **não** reverter Vortex; dono = agente único |

### Dragons

| # | Resposta |
|---|----------|
| 1 | Sim — differentiator |
| 2 | Sim via Torre |
| 3 | Sim (UI/FSM) |
| 4 | **Sim** — mesh placeholder |
| 5 | Não |
| 6 | Deploy/combate parcial OK para beta |
| 7 | Não |
| 8 | **Continuar** arte + presença mapa; não expandir PvP |

### World Map / marchas / criaturas / território

| # | Resposta |
|---|----------|
| 1 | Sim para núcleo beta |
| 2 | Sim |
| 3 | Sim |
| 4 | **Sim** |
| 5 | Não |
| 6 | Overlay território sem guerra = OK fundação |
| 7 | Risco se alguém implementar aliança/guerra agora |
| 8 | **Continuar** polish beta; **parar** alianças/guerra até pós-validação |

### Alianças / PvP / SvS / Capital / Rei

| # | Resposta |
|---|----------|
| 1 | No plano **longo prazo**, não na Beta 0.1 |
| 2 | Não |
| 3 | Não |
| 4 | N/A |
| 5 | N/A |
| 6 | **Seria cedo** se iniciado agora |
| 7 | Não (ainda inexistente) |
| 8 | **Parar** / não iniciar até sprint dedicado |

### Backend / Admin / Infra

| # | Resposta |
|---|----------|
| 1 | Alinhado ao monorepo README; **desalinhado** ao runtime beta |
| 2 | Não no player |
| 3 | Não |
| 4 | Admin dashboard stub |
| 5 | Persistência local **vs** API = dois mundos até sync |
| 6 | Backend cedo foi decisão D001/D002 — aceitável como fundação |
| 7 | **Desvio documental**: README sugere online completo |
| 8 | **Corrigir docs** ou **sprint integração**; não fingir online no exe |

### Monetização

| # | Resposta |
|---|----------|
| 1 | Fora da beta (D008) |
| 2–7 | N/A / não iniciar |
| 8 | **Parar** |

### Android / iOS

| # | Resposta |
|---|----------|
| 1 | Alvo de produto; não da beta técnica Windows |
| 2–3 | Não |
| 8 | **Continuar** só após estabilidade Windows + arte mínima |

---

## DESVIOS DO PLANO

```text
DESVIOS DO PLANO
================

D1. RUNTIME OFFLINE vs README ONLINE
    Evidência: player usa PlayerPrefs; auditoria HTML 2026-07-26;
               server Auth/Heroes sem chamada no fluxo beta.
    Ação: atualizar README "Estado atual" OU sprint de integração API.
    Severidade: ALTA (expectativa de produto).

D2. ARTE PROVISÓRIA EM MÓDULOS CORE (City / Map / Dragons / heroínas)
    Evidência: silhouettes, dummies, dragons placeholder;
               README "sem protótipos descartáveis" vs placeholders admitidos.
    Ação: tratar placeholders como débito visual explícito (não como feature).
    Severidade: MÉDIA — permitido na beta técnica se rastreado.

D3. DUPLA NOMENCLATURA DE BUILD (Checkpoint vs Beta-0.1)
    Evidência: builds/windows/Valgor-Checkpoint e Valgor-Beta-0.1;
               docs misturam nomes na história.
    Ação: Beta-0.1 oficial (D021); Checkpoint só legado.
    Severidade: BAIXA.

D4. DESIGN-REFERENCES SEM ASSETS
    Evidência: INDEX + LAST_Z map; 0 imagens no diretório.
    Ação: popular refs ou marcar pasta como mapa textual only.
    Severidade: BAIXA.

D5. WORKERS / ADMIN / NGINX INCOMPLETOS
    Evidência: stubs; admin dashboard stub.
    Ação: não priorizar até client→API; evitar feature creep infra.
    Severidade: BAIXA para beta jogável.

D6. AUDITORIA 26/07 vs RELEASES 27/07
    Evidência: HTML dizia build quebrado; releases 27/07 documentam recovery.
    Ação: este documento + releases prevalecem sobre HTML desatualizado;
          manter HTML como histórico.
    Severidade: DOCUMENTAL.

NÃO É DESVIO (escopo consciente):
- Alianças / PvP / SvS / gacha / shooter ausentes (D008).
- Windows-only na beta (mobile é alvo futuro).
- Menu contextual de edifícios (alinhado, não desvio).
```

---

## Governança (atualização 2026-07-27)

**Agente único (D022):** responsável por Heroes + resto do monorepo. Segundo agente descontinuado.  
Ver `VALGOR_SINGLE_AGENT.md`. Sobreposição antiga “jogo vs heróis” deixa de existir como processo; permanece só a fronteira de módulos.

---

## Resumo executivo

| Dimensão | Veredito 2026-07-27 |
|----------|---------------------|
| Direção de produto | **Seguindo** o plano mestre (cidade→heróis→dragões→mapa) |
| Beta 0.1 jogável Windows | **Sim** (evidência releases + exe) |
| Completude vs visão mobile online | **Longe** — offline + arte provisional + sem alianças/endgame |
| Risco principal | Expandir features sociais/PvP **antes** de arte/integração/mobile |
| Governança | Agente único (D022) |
| Próximo passo | Ver `VALGOR_NEXT_SPRINT.md` |

---

## Commits de referência (amostra — início → 2026-07-27)

Fundação monorepo → game-core → city → heroes foundation → Vortex → dragons → world-map → jornada/save → limpeza URP/input/localização → BuildingContext UX (`ac83ebe`).  
Histórico completo: `git log --oneline` no repositório.

---

## Evidência sessão 2026-07-27 (consolidação D022)

- Server tests (`dotnet test server/Valgor.sln`): **aprovados 23 / falha 0 / total 23** (Domain 15 + Application 4 + Api 4).
- GameLogic tests (`tools/Valgor.GameLogic.Tests`): **não executaram** — falha de compilação CS0234 (`Valgor.UI` ausente em `WorldMapSession.cs`); preexistente, fora do escopo D022.
- Build Beta 0.1: `builds/windows/Valgor-Beta-0.1/Valgor.exe` existe (Length=672256, LastWriteTime=2026-07-27 12:19:09); Start-Process OK, processo Valgor ativo ~9s, Stop-Process graceful OK.
