# VALGOR — Log de Decisões

**Documento:** `docs/project-control/VALGOR_DECISIONS_LOG.md`  
**Atualizado:** 2026-07-27  
**Fontes de prova:** docs versionados, CHANGELOG, commits, código — não chat.

| ID | Data | Tema | Decisão | Motivo | Impacto | Status |
|----|------|------|---------|--------|---------|--------|
| D001 | 2026-07-25 | Monorepo | Fundação única `client` + `server` + `admin` + `docs` + Docker | Evitar protótipos descartáveis (README) | Toda entrega vive no mesmo repositório | Vigente |
| D002 | 2026-07-25 | Backend | Clean Architecture .NET 9 + JWT + EF + Compose | Produção desde o dia 1 | API Auth/Heroes; Workers stub | Vigente |
| D003 | 2026-07-26 | Heróis | Vortex é o herói principal e ícone da franquia | Bíblia `VALGOR_HEROES_MASTER.md` | Pipeline `production/Vortex`, preview HeroesDemo | Vigente |
| D004 | 2026-07-26 | Heróis | Apenas Vortex masculino no elenco principal atual | Direção narrativa/franquia | 10 heroínas + 1 Vortex no seed | Vigente |
| D005 | 2026-07-26 | Heróis | Heroínas adultas em estilo medieval-fantástico | Identidade vs estética contemporânea/zumbi | Arte/UI devem respeitar o tema | Vigente |
| D006 | 2026-07-26 | Heróis | Poderes especiais ativos com duração e cooldown | Combate/estratégia data-driven | Client simulation + API `battle/special/activate` | Vigente |
| D007 | 2026-07-26 | Referências | Last Z e refs externas = **inspiração de sistemas**, não cópia | INDEX design-references | Sem zumbis, sem shooter na beta | Vigente |
| D008 | 2026-07-26 | Escopo beta | Shooter / gacha / aliança / PvP **fora** da Beta 0.1 | `LAST_Z_SYSTEMS_MAP.md` | Foco cidade↔heróis↔dragões↔mapa | Vigente |
| D009 | 2026-07-26 | Differentiator | Torre dos Dragões como eixo (não existe em Last Z) | Posicionamento de produto | Módulo Dragons + slot City | Vigente |
| D010 | 2026-07-26 | Arte | Modelos reais aprovados **não** podem voltar a dummy | Sprint Vortex / qualidade de franquia | Vortex FBX obrigatório no player quando possível | Vigente |
| D011 | 2026-07-26 | Métrica | Evolução medida pelo **executável** e evidências, não por nº de classes | Auditoria checkpoint | Smoke + PNGs + Build Successful | Vigente |
| D012 | 2026-07-26 | Qualidade | Features novas **congelam** se houver bloqueio visual crítico | Evitar dívida magenta/missing/scripts | Priorizar recovery visual | Vigente |
| D013 | 2026-07-26 | Persistência beta | Save local PlayerPrefs (`valgor.player.v1`, dragons.v3, city, worldmap) | Beta offline | Continuar / Novo Jogo sem API | Vigente |
| D014 | 2026-07-26 | Integração | Client beta **não** depende da API no fluxo jogável | Destravar validação Windows | README diagrama ≠ runtime beta | Vigente (revisar pós-beta) |
| D015 | 2026-07-26 | Build | Build Windows a partir de `client/` real; scaffold `_unity-beta-project` obsoleto | Auditoria / Package Manager ENOENT | Scripts `build-windows-beta.ps1` | Vigente |
| D016 | 2026-07-27 | Cidade | Cidade como tela inicial pós-intro | Jornada Beta 0.1 | MainMenu → City | Vigente |
| D017 | 2026-07-27 | Cidade | Interação contextual direta nos edifícios; **sem** menu central de construções | UX produto / `city-building-context-ux.md` | `BuildingContext*` + presenter | Vigente |
| D018 | 2026-07-27 | Localização | PT-BR embutido; sem catálogo Addressables na beta | Evitar InvalidKey / warnings | `LocalizationBootstrap` skip package init | Vigente |
| D019 | 2026-07-27 | Input | Input System only (`activeInputHandler: 1`) | Remover probe XInput1_3 | Teclado/mouse; doc INPUT_SYSTEM | Vigente |
| D020 | 2026-07-27 | Render | Corrigir GUIDs URP stub (Missing Script no boot) | Limpeza Beta 0.1 | Assets `_Valgor/Settings/*` | Vigente |
| D021 | 2026-07-27 | Build pasta | Saída oficial `builds/windows/Valgor-Beta-0.1` | Alinhar docs e `ValgorVersion.BuildFolderName` | Checkpoint = legado/espelho | Vigente |
| D022 | 2026-07-27 | Agentes | **Agente único** assume o monorepo inteiro; segundo agente (heróis) descontinuado | Evitar conflito, duplicação e regressão | Um dono; fronteiras de pasta/módulo permanecem como arquitetura | Vigente |
| D023 | 2026-07-27 | Build 0.2.1 | Pasta oficial atual `Valgor-Beta-0.2.1`; **congelar** 0.1 e 0.2 | Fechamento P0/P1 sem sobrescrever builds auditadas | `ValgorVersion` + `build-windows-beta-0.2.1.ps1` | Vigente |
| D024 | 2026-07-27 | Pós-0.2.1 | Próximo foco = vertical slice **visual**; sem PvP/alianças/loja | Auditoria visual gap P2 | Não abrir novos sistemas de jogo nesta etapa | Supersedida em parte por D025 |
| D025 | 2026-07-29 | Dragões Fase 1 | Jornada oficial do ovo: Castelo≥20 → missão → conquista → incubação+care → nascimento Nv.1; persistência `valgor.dragons.v4`; um dragão | City homologada; diferenciador Torre | Sem combate/PvP/montaria/múltiplos dragões | Vigente |
| D026 | 2026-07-29 | Dragões Fase 2 | Progressão Nv.1→30 com XP, caps Castelo/Torre, rituais 6/11/16/21/26, energia/saúde, timers e acelerador; persistência `valgor.dragons.v5` (migra v4) | Continuação do módulo Dragão | UI Torre: Evoluir/Acelerar; sem combate completo | Vigente |
| D027 | 2026-07-29 | Dragões Fase 2 P1 | Visuais por estágio data-driven (`DragonStageVisualConfig`), troca só ao concluir ritual, E2E Unity `-dragonPhase2E2E` Nv.1→30 | Fechamento P1 Fase 2 | Placeholders próprios até asset definitivo; sem Fase 3 | Vigente |
| D028 | 2026-07-30 | Dragões Fase 3 | Habilidades (3 slots) + combate PvE como suporte automático; energia/saúde; recall ferido; persistência `valgor.dragons.v6` | Continuação do módulo Dragão | Sem PvP/montaria/múltiplos/controle manual | Vigente |

---

## Notas

- Decisões de **produto longo prazo** (alianças, Capital, Rei do Reino, Android/iOS store) estão no plano mestre mas **ainda sem implementação** — não confundir com decisões da Beta 0.1.
- Conflito documentado: README promete Unity→API; D014 mantém offline até sprint de integração explícita.
- Carta operacional: `VALGOR_SINGLE_AGENT.md` + regra Cursor `.cursor/rules/valgor-single-agent.mdc`.
- Persistência dragões: D013 citava `dragons.v3`; D025 promove `dragons.v4` (jornada do ovo).
