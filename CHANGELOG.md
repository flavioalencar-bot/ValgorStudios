# Changelog

Todas as mudan?as relevantes deste reposit?rio s?o documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/),
e o projeto adere a [SemVer](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Added

- **Dragão Fase 1 (ovo e incubação):** Castelo Nv.20 desbloqueia conteúdo; missão/conquista/incubação com cuidados na Torre; nascimento Nv.1; persistência `valgor.dragons.v4`; testes `DragonFoundation` 20/0

### Added

- **Polish visual modais de evolucao:** preview 3D RenderTexture, icones Valgor, molduras/botoes refinados; build `Valgor-QA-Building-Upgrade-Visual`

### Added

- **UX evolu??o de edif?cios:** modais centrais Detalhes / Atualizar / Obter mais / Reabastecimento autom?tico; invent?rio de pacotes (sem loja); view-models data-driven; build QA `Valgor-QA-Building-Upgrade-UX`

### Fixed

- **Castelo Tier 1:** materiais URP/Lit + BaseMap; BuildingView nao apaga texturas do asset real

### Added

- **Beta 0.2.2:** Castelo Tier 1 asset real (GLB/FBX), pasta isolada `Valgor-Beta-0.2.2`, fallback procedural so se o asset falhar

### Changed

- **Castelo Tier 1:** visual alinhado ÿ referÿncia oficial (brasÿo na porta/bandeiras); acentos preservados no tint de seleÿÿo

### Added

- **Beta 0.2.1** pasta isolada `Valgor-Beta-0.2.1` (preserva 0.1 e 0.2); script `build-windows-beta-0.2.1.ps1`; revalida??o P0/P1

### Fixed

- Her?is Lyra/Nyx/Selene: `PendingNamePlaceholder` incorreto fazia cards exibirem o t?tulo
- **Patch de recuperacao Beta 0.2 (P0/P1):** energia World Map, menu/nav, missoes minimas, save diagnostics, build sem watermark Development

### Added

- **Beta 0.2** vertical slice: vers?o/`Valgor-Beta-0.2`, HUD de construtores, wipe inclui `wall`, painel Drag?es acion?vel, arte modular dos 8 edif?cios restantes, nomes das 3 hero?nas pendentes, script `build-windows-beta-0.2.ps1`
- Doc: `VALGOR_BETA_0_2_MACRO.md` (escopo inferido; checklist original truncado no chat)

### Fixed

- City: Muralha clic?vel ? colliders nos segmentos/port?es + `BuildingSelectionClickProxy` ? `wall`; destaque da fortifica??o inteira; log `Building clicked: wall`
- City: **Concluir Agora** s? habilitado durante constru??o ativa

### Added

- City: **Muralha** como edif?cio evolutivo (`wall`) ? Detalhes/Atualizar, requisitos Castelo?N, efeitos de defesa/HP/resist?ncia exibidos, visual do anel/port?es por n?vel; evid?ncias `ux-31`?`ux-35`

### Fixed

- City: dire??o visual P0 ? UV/tiling em espa?o de mundo, caminhos de pedra (n?o t?buas), horizonte/n?voa sem fundo azul, Castelo dominante, Torre circular leg?vel, telhados/cercas/planta??es proporcionados; evid?ncias `art-direction-p0/`

### Added

- City: arte m?nima P0 ? Castelo, Torre dos Drag?es, Fazenda, Armaz?m, Academia (modulares reconhec?veis); materiais com noise; indicadores medalh?o/chevron; evid?ncias `art-01`?`art-09`
- Visual: invent?rio de placeholders `VALGOR_PLACEHOLDER_INVENTORY.md`; evid?ncias `visual-consolidation/`
- City: distritos (economia/militar/com?rcio/m?stico), port?es, pads de zona; sele??o dourada; detalhes dourados Castelo/Torre/Serraria
- Main Menu: bot?o **Jogar**, selo Beta 0.1 offline; tema Success/Danger
- World Map: territ?rios de leitura; Dragons ninho sem c?psula roxa

### Added

- City: UX contextual 3? entrega ? Arena, Hospital, Torre dos Drag?es, Templo, Mercado, Laborat?rio (`SupportBuildingRules`; Torre usa `IDragonGateway`)
- City: UX contextual 2? entrega ? Serraria, Pedreira, Mina (Coletar/Detalhes/Atualizar) e Academia (Detalhes/Atualizar); `ProductionBuildingDetails` (taxa, estoque, capacidade, tempo at? lotar)
- City: deps data-driven Serraria?Fazenda, Pedreira?Serraria, Mina?Pedreira, Academia?Armaz?m; Arena/Torre/Lab?Academia; Hospital?Fazenda/Armaz?m; Templo?Hospital; Mercado?Armaz?m; Lab?Mina
- City: pr?-requisitos data-driven de evolu??o (`BuildingRequirementCatalog` + `BuildingRequirementEvaluator`) ? Castelo/pr?dios/pesquisa; painel Atualizar com Pr?-requisitos (vermelho + **Ir**), Recursos e **Atualizar** bloqueado quando inv?lido
- Regras iniciais: Fazenda?Castelo N; Armaz?m?Castelo N + Fazenda; Castelo?Fazenda/Armaz?m do cat?logo
- Doc: `docs/architecture/city-building-upgrade-requirements.md`

### Fixed

- City: `GetCastleLevel()` nos pr?-requisitos usa s? o n?vel do edif?cio Castelo na cidade (ignora PlayerLevel / `BetaProgress`); evid?ncias `ux-13` / `ux-11` / `ux-14`
- City: bot?o **Detalhes** do menu contextual n?o abria o painel ? o mesmo clique re-selecionava o pr?dio e fechava o modal; painel dedicado `BuildingDetailsPanel` + suppress do raycast 3D
- UX contextual: tutorial n?o cobre painel de atualiza??o (lado oposto + Recolher); menu afastado do pr?dio; indicadores verdes com ?cone de recurso (quantidade s? ao selecionar); painel upgrade com scroll/bot?es est?veis em 1600?900
- City: sele??o de edif?cios quebrada com Input System only ? `OnMouse*` n?o dispara; adicionado `CityBuildingPointerInput` (raycast), layer `Building`, colliders; tutorial overlay n?o bloqueia a cidade

### Added

- Sprint UX contextual (1? entrega): Castelo (Detalhes/Atualizar), Fazenda (Coletar/Detalhes/Atualizar), Armaz?m (Abrir/Detalhes/Atualizar)
- Painel de atualiza??o com Ouro/Comida/Madeira/Pedra/Ferro/Ess?ncia (?/?), **Concluir Agora** (diamantes), progresso 3D no pr?dio
- `WarehouseRules` (capacidade/prote??o), coleta direta no indicador da Fazenda, bot?es circulares no menu contextual
- Evid?ncias smoke `ux-01`?`ux-10` no `CheckpointSmokeDriver`

### Fixed

- GameLogic: removida depend?ncia `Valgor.UI` de `WorldMapSession` (evento `RewardDeposited` ? HUD/tutorial); testes `tools/Valgor.GameLogic.Tests` voltam a compilar (**114/114**)

### Changed

- C?mera da cidade: arrastar com bot?o esquerdo/toque move o mapa sem selecionar pr?dio; pin?a/scroll zoom
- Custos de upgrade passam a incluir Food/Iron/Ess?ncia no cat?logo; diamantes iniciais na carteira seed (50)
- `production/Vortex/`: versionados exports FBX, staging Unity e previews de propor??o; `.gitignore` cobre `.blend1`, autosaves, `__pycache__`, scripts tempor?rios `_inspect*`/`_tmp*` e dumps locais
- Governan?a: **agente ?nico** assume o monorepo (Game Core + City + Heroes + Dragons + Map + backend + docs); segundo agente de her?is descontinuado ? `docs/project-control/VALGOR_SINGLE_AGENT.md`, decis?o D022

### Added

- UX contextual de edif?cios na City: menu ancorado ao pr?dio (`BuildingContextMenu`), presenter, positioner e pain?is de a??o (Detalhes/Atualizar/Coletar/Produzir/Treinar/Pesquisar/Abrir/Enviar)
- `CityCameraController.FocusOn` com centraliza??o suave ao selecionar edif?cio

### Fixed

- Missing Scripts no boot: assets URP stubados (`UniversalRenderPipelineAsset` / `UniversalRenderer`) com GUIDs inv?lidos ? restaurados para GUIDs URP 17
- Localiza??o Beta 0.1: PT-BR embutido sem Addressables; warnings de cat?logo removidos
- Input: handler **Input System only** (sem probe legado `XInput1_3.dll`)

### Changed

- Jornada Beta 0.1: intro Vortex (4 frases), tutorial 11 passos, save/Continuar com ?ltima tela, build `Valgor-Beta-0.1`
- Progress?o beta: `BetaProgress` (Castelo + pesquisa Coleta); poder Vortex escala com Castelo; gather x1.10 (+5% com Lab); fila de marcha 1 ativa + 1 enfileirada com feedback no HUD
- UX estilo Last Z (beta): **persist?ncia de n?veis** dos pr?dios, **fila de constru??o 1/1** com timer curto, bolhas de coleta com valor, setas de upgrade, faixa Constru??o/Pesquisa no HUD; no mapa badges de n?vel + path pontilhado da marcha

### Fixed

- UI Toolkit da beta: `PanelSettings` sem Theme Style Sheet. Hosts aplicam tema ao criar `UIDocument`; `BetaUiPanels` substitui settings sem tema

### Changed

- Cidade provisional com silhuetas distintas por edif?cio, layout em distritos (castelo central), pra?a/caminhos/muralha e ambiente ? ainda sem arte final FBX
- World Map provisional: silhuetas por tipo de n?, terreno/atmosfera e **marcha vis?vel** no mundo (`MarchArmyView` com trail)
- Torre dos Drag?es: ninho 3D com ocupantes; upgrade gated pelo n?vel do Castelo (estilo HQ); `BetaHeroesGateway` (Vortex poder 280) nos encontros; painel de combate no mapa
- Beta 0.1 fechada como jogo strategy jog?vel: poder no HUD, deploy de drag?o com feedback, dep?sito de loot, tutorial sem pular retorno, PLACEHOLDERs limpos, forma??o Vortex no HeroesDemo

### Added

- Mapa de sistemas Last Z ? Valgor (`docs/design-references/LAST_Z_SYSTEMS_MAP.md`): o que entra na beta, o que fica de fora (shooter/gacha/alian?a)
- Jornada do jogador Beta T?cnica 0.1: splash/loading com mensagens, first-access (nome+ID local), intro, tutorial guiado na cidade/mapa, Continuar/Nova Jornada via `LocalPlayerProfile`
- Vortex jog?vel: rig Humanoid no Blender, 16 clips m?nimos, `Vortex_DragonSword.fbx`, Avatar Humanoid no Unity, espada em `Socket_RightHand`, VFX Dom?nio do Rei (~10s) e preview HeroesDemo sem dummy
- Script de produ??o `production/Vortex/rig_animate_weapon_vortex.py` (skinning + anima??es + export FBX)

### Added

- Dragon Foundation em `Assets/Valgor/Dragons`: defini??o/inst?ncia, state machine, ninho, alimenta??o, recupera??o, deployment em marchas e `IDragonGateway`
- Integra??o City (Torre dos Drag?es) + World Map (destaque/recall/poder provis?rio) + recursos (Food/Essence)
- Documenta??o `docs/architecture/dragons.md`
- Complemento 02 de drag?es: estados oficiais (EGG/JUVENILE/DEPLOYED/?), `DragonHungerService`, descanso e recupera??o com timers
- Complemento 03 de drag?es: est?gios de crescimento (HATCHLING?ANCIENT), v?nculo e evolu??o de esp?cie
- Beta T?cnica 0.1: fluxo Bootstrap?Loading?MainMenu?City?Heroes?Torre?WorldMap?City, navega??o provis?ria, identidade visual e build Windows

### Added

- Pipeline do her?i real **Vortex** (`Assets/Valgor/Heroes/Characters/Vortex/`): pastas, import profile, validators, menus `Valgor/Heroes/Vortex/*`, prefab shell `Vortex_Hero`, Animator Controller, materiais URP placeholder, Addressable key `heroes/HERO_VORTEX_000/prefab`, fallback t?cnico at? o FBX final, postprocessor de auto-build
- Preview 360° resolve Vortex via `HeroVisualResolver` (prefab real ou fallback) e dispara anima??o/VFX de poder especial no bot?o da demo

### Added (anterior)

- Game Core Foundation no cliente Unity: `ServiceRegistry`, `GameSession`, `GameStateMachine`, `LoadingFlow`, `GameNavigator`
- Fluxo de cenas: Bootstrap ? Loading ? MainMenu ? City ? WorldMap ? City
- Cenas provis?rias `City` e `WorldMap` com hosts de UI Toolkit
- Contratos de integra??o de m?dulos (`IPlayerCityModule`, `IWorldMapModule`, `IBuildingModule`, `IResourceModule`, `IDragonModule`, `IHeroesGateway`)
- Testes de l?gica em `tools/Valgor.GameLogic.Tests`
- Documenta??o `docs/architecture/game-core.md`
- Valgor Player City Foundation em `Assets/Valgor/City`: cat?logo e slots de 14 edif?cios, carteira de recursos, sele??o, melhorias, c?mera isom?trica e HUD
- Produ??o passiva online/offline (12h), coleta, capacidade por n?vel, persist?ncia local e tick determin?stico
- World Map Foundation em `Assets/Valgor/WorldMap`: regi?es selecion?veis, c?mera, HUD e retorno ? cidade
- World Map Interaction: n?s tipados (cidade/vilarejo/recurso/criatura/drag?o/marco), marcha provis?ria com tempo de deslocamento, coleta de recursos, persist?ncia City?WorldMap e contrato `IHeroesGateway.TryReserveMarchSlot`
- Criaturas do World Map: `WorldCreatureDefinition`/`Instance`, `CreatureRewardTable`, `CreatureDifficultyResolver`, `CreatureEncounterService` (engajar, resolver provis?rio, respawn, energia)
- Marchas completas e ocupa??o de n?s: `MarchStateMachine`, `MarchService`, `MarchRepository`, `MarchTravelCalculator`, `WorldNodeOccupationService`, `MarchChangedEvent`
- Coleta completa no mapa: taxa/`gatherRatePerHour`, carga da marcha, deple??o e respawn de n?s de recurso (`WorldResourceGatheringService`)
- Energia do mapa mundial: `PlayerEnergyWallet`, regen por timestamp, custos configur?veis, HUD com ETA e persist?ncia dedicada (`EnergyPersistenceRepository`)
- Filtros, localizar e vis?o territorial no World Map: visibilidade combin?vel, foco de c?mera com limites, overlays Neutral/Owned/Allied/Enemy/Contested/Locked
- Patch de restaura??o do World Map: c?mera/zoom, sele??o por ID, tick global de marchas e dep?sito de carteira at?mico (`rewardDeliveryId`/`IsCommitted`)
- Documenta??o `docs/architecture/player-city.md` e `docs/architecture/world-map.md`

## [0.2.0] ? 2026-07-26

### Added

- Sistema de her?is (Vortex + 10 hero?nas) orientado a `docs/game-design/heroes/heroes.seed.json`
- Backend: cat?logo, fac??es, vantagem circular (+15%), b?nus 3/3+2/4/5, poderes READY/ACTIVE/COOLDOWN, roster do jogador, valida??o de equipe
- Endpoints `/api/heroes/*`, `/api/players/me/heroes`, `/api/teams/validate`, `/api/battle/.../special/activate`
- Migration EF `AddHeroesSystem` e seed autom?tico do cat?logo
- Unity: `Assets/Valgor/Heroes/` (Data, Factions, SpecialPowers, Magic, Skins, UI, Preview360, placeholders Addressables)
- Testes de dom?nio/aplica??o para vantagem, b?nus, cooldown, idempot?ncia e nomes ?A definir?

## [0.1.0] ? 2026-07-25

### Added

- Monorepo oficial Valgor Studios (client, server, admin, database, infra, docs, assets, tools)
- Backend .NET 9 em Clean Architecture com MediatR 12.4.1, FluentValidation, Result Pattern, Domain Events e BaseEntity
- Autentica??o JWT (`POST /api/auth/login`)
- Endpoints de sistema `GET /health` e `GET /version`
- Swagger, Serilog, HealthChecks (PostgreSQL, Redis, EF Core)
- Middleware global de exce??es e Validation Pipeline
- Entity Framework Core + Npgsql com migration `InitialCreate` e seed de admin em Development
- Redis cache e Docker Compose (PostgreSQL 5437, Redis 6383, pgAdmin 5051)
- Admin React + Vite + TypeScript com login, dashboard, menu lateral, tema e auth JWT
- Client Unity 6 LTS com URP, Addressables, Input System, Localization, pooling, ?udio, scene loader e loading screen
- GitHub Actions de Build e Test do backend

### Security

- Hash de senha PBKDF2 (SHA-256, 100k itera??es)
- JWT com valida??o de issuer, audience e lifetime
- MediatR 12.4.1 (licen?a Apache-2.0) ? sem depend?ncia comercial

