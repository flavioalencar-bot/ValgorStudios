# Changelog

Todas as mudanças relevantes deste repositório são documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/),
e o projeto adere a [SemVer](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Added

- Visual: inventário de placeholders `VALGOR_PLACEHOLDER_INVENTORY.md`; evidências `visual-consolidation/`
- City: distritos (economia/militar/comércio/místico), portões, pads de zona; seleção dourada; detalhes dourados Castelo/Torre/Serraria
- Main Menu: botão **Jogar**, selo Beta 0.1 offline; tema Success/Danger
- World Map: territórios de leitura; Dragons ninho sem cápsula roxa

### Added

- City: UX contextual 3ª entrega — Arena, Hospital, Torre dos Dragões, Templo, Mercado, Laboratório (`SupportBuildingRules`; Torre usa `IDragonGateway`)
- City: UX contextual 2ª entrega — Serraria, Pedreira, Mina (Coletar/Detalhes/Atualizar) e Academia (Detalhes/Atualizar); `ProductionBuildingDetails` (taxa, estoque, capacidade, tempo até lotar)
- City: deps data-driven Serraria←Fazenda, Pedreira←Serraria, Mina←Pedreira, Academia←Armazém; Arena/Torre/Lab←Academia; Hospital←Fazenda/Armazém; Templo←Hospital; Mercado←Armazém; Lab←Mina
- City: pré-requisitos data-driven de evolução (`BuildingRequirementCatalog` + `BuildingRequirementEvaluator`) — Castelo/prédios/pesquisa; painel Atualizar com Pré-requisitos (vermelho + **Ir**), Recursos e **Atualizar** bloqueado quando inválido
- Regras iniciais: Fazenda→Castelo N; Armazém→Castelo N + Fazenda; Castelo→Fazenda/Armazém do catálogo
- Doc: `docs/architecture/city-building-upgrade-requirements.md`

### Fixed

- City: `GetCastleLevel()` nos pré-requisitos usa só o nível do edifício Castelo na cidade (ignora PlayerLevel / `BetaProgress`); evidências `ux-13` / `ux-11` / `ux-14`
- City: botão **Detalhes** do menu contextual não abria o painel — o mesmo clique re-selecionava o prédio e fechava o modal; painel dedicado `BuildingDetailsPanel` + suppress do raycast 3D
- UX contextual: tutorial não cobre painel de atualização (lado oposto + Recolher); menu afastado do prédio; indicadores verdes com ícone de recurso (quantidade só ao selecionar); painel upgrade com scroll/botões estáveis em 1600×900
- City: seleção de edifícios quebrada com Input System only — `OnMouse*` não dispara; adicionado `CityBuildingPointerInput` (raycast), layer `Building`, colliders; tutorial overlay não bloqueia a cidade

### Added

- Sprint UX contextual (1ª entrega): Castelo (Detalhes/Atualizar), Fazenda (Coletar/Detalhes/Atualizar), Armazém (Abrir/Detalhes/Atualizar)
- Painel de atualização com Ouro/Comida/Madeira/Pedra/Ferro/Essência (✓/✗), **Concluir Agora** (diamantes), progresso 3D no prédio
- `WarehouseRules` (capacidade/proteção), coleta direta no indicador da Fazenda, botões circulares no menu contextual
- Evidências smoke `ux-01`…`ux-10` no `CheckpointSmokeDriver`

### Fixed

- GameLogic: removida dependência `Valgor.UI` de `WorldMapSession` (evento `RewardDeposited` → HUD/tutorial); testes `tools/Valgor.GameLogic.Tests` voltam a compilar (**114/114**)

### Changed

- Câmera da cidade: arrastar com botão esquerdo/toque move o mapa sem selecionar prédio; pinça/scroll zoom
- Custos de upgrade passam a incluir Food/Iron/Essência no catálogo; diamantes iniciais na carteira seed (50)
- `production/Vortex/`: versionados exports FBX, staging Unity e previews de proporção; `.gitignore` cobre `.blend1`, autosaves, `__pycache__`, scripts temporários `_inspect*`/`_tmp*` e dumps locais
- Governança: **agente único** assume o monorepo (Game Core + City + Heroes + Dragons + Map + backend + docs); segundo agente de heróis descontinuado — `docs/project-control/VALGOR_SINGLE_AGENT.md`, decisão D022

### Added

- UX contextual de edifícios na City: menu ancorado ao prédio (`BuildingContextMenu`), presenter, positioner e painéis de ação (Detalhes/Atualizar/Coletar/Produzir/Treinar/Pesquisar/Abrir/Enviar)
- `CityCameraController.FocusOn` com centralização suave ao selecionar edifício

### Fixed

- Missing Scripts no boot: assets URP stubados (`UniversalRenderPipelineAsset` / `UniversalRenderer`) com GUIDs inválidos — restaurados para GUIDs URP 17
- Localização Beta 0.1: PT-BR embutido sem Addressables; warnings de catálogo removidos
- Input: handler **Input System only** (sem probe legado `XInput1_3.dll`)

### Changed

- Jornada Beta 0.1: intro Vortex (4 frases), tutorial 11 passos, save/Continuar com última tela, build `Valgor-Beta-0.1`
- Progressão beta: `BetaProgress` (Castelo + pesquisa Coleta); poder Vortex escala com Castelo; gather x1.10 (+5% com Lab); fila de marcha 1 ativa + 1 enfileirada com feedback no HUD
- UX estilo Last Z (beta): **persistência de níveis** dos prédios, **fila de construção 1/1** com timer curto, bolhas de coleta com valor, setas de upgrade, faixa Construção/Pesquisa no HUD; no mapa badges de nível + path pontilhado da marcha

### Fixed

- UI Toolkit da beta: `PanelSettings` sem Theme Style Sheet. Hosts aplicam tema ao criar `UIDocument`; `BetaUiPanels` substitui settings sem tema

### Changed

- Cidade provisional com silhuetas distintas por edifício, layout em distritos (castelo central), praça/caminhos/muralha e ambiente — ainda sem arte final FBX
- World Map provisional: silhuetas por tipo de nó, terreno/atmosfera e **marcha visível** no mundo (`MarchArmyView` com trail)
- Torre dos Dragões: ninho 3D com ocupantes; upgrade gated pelo nível do Castelo (estilo HQ); `BetaHeroesGateway` (Vortex poder 280) nos encontros; painel de combate no mapa
- Beta 0.1 fechada como jogo strategy jogável: poder no HUD, deploy de dragão com feedback, depósito de loot, tutorial sem pular retorno, PLACEHOLDERs limpos, formação Vortex no HeroesDemo

### Added

- Mapa de sistemas Last Z → Valgor (`docs/design-references/LAST_Z_SYSTEMS_MAP.md`): o que entra na beta, o que fica de fora (shooter/gacha/aliança)
- Jornada do jogador Beta Técnica 0.1: splash/loading com mensagens, first-access (nome+ID local), intro, tutorial guiado na cidade/mapa, Continuar/Nova Jornada via `LocalPlayerProfile`
- Vortex jogável: rig Humanoid no Blender, 16 clips mínimos, `Vortex_DragonSword.fbx`, Avatar Humanoid no Unity, espada em `Socket_RightHand`, VFX Domínio do Rei (~10s) e preview HeroesDemo sem dummy
- Script de produção `production/Vortex/rig_animate_weapon_vortex.py` (skinning + animações + export FBX)

### Added

- Dragon Foundation em `Assets/Valgor/Dragons`: definição/instância, state machine, ninho, alimentação, recuperação, deployment em marchas e `IDragonGateway`
- Integração City (Torre dos Dragões) + World Map (destaque/recall/poder provisório) + recursos (Food/Essence)
- Documentação `docs/architecture/dragons.md`
- Complemento 02 de dragões: estados oficiais (EGG/JUVENILE/DEPLOYED/…), `DragonHungerService`, descanso e recuperação com timers
- Complemento 03 de dragões: estágios de crescimento (HATCHLING…ANCIENT), vínculo e evolução de espécie
- Beta Técnica 0.1: fluxo Bootstrap→Loading→MainMenu→City→Heroes→Torre→WorldMap→City, navegação provisória, identidade visual e build Windows

### Added

- Pipeline do herói real **Vortex** (`Assets/Valgor/Heroes/Characters/Vortex/`): pastas, import profile, validators, menus `Valgor/Heroes/Vortex/*`, prefab shell `Vortex_Hero`, Animator Controller, materiais URP placeholder, Addressable key `heroes/HERO_VORTEX_000/prefab`, fallback técnico até o FBX final, postprocessor de auto-build
- Preview 360° resolve Vortex via `HeroVisualResolver` (prefab real ou fallback) e dispara animação/VFX de poder especial no botão da demo

### Added (anterior)

- Game Core Foundation no cliente Unity: `ServiceRegistry`, `GameSession`, `GameStateMachine`, `LoadingFlow`, `GameNavigator`
- Fluxo de cenas: Bootstrap → Loading → MainMenu → City → WorldMap → City
- Cenas provisórias `City` e `WorldMap` com hosts de UI Toolkit
- Contratos de integração de módulos (`IPlayerCityModule`, `IWorldMapModule`, `IBuildingModule`, `IResourceModule`, `IDragonModule`, `IHeroesGateway`)
- Testes de lógica em `tools/Valgor.GameLogic.Tests`
- Documentação `docs/architecture/game-core.md`
- Valgor Player City Foundation em `Assets/Valgor/City`: catálogo e slots de 14 edifícios, carteira de recursos, seleção, melhorias, câmera isométrica e HUD
- Produção passiva online/offline (12h), coleta, capacidade por nível, persistência local e tick determinístico
- World Map Foundation em `Assets/Valgor/WorldMap`: regiões selecionáveis, câmera, HUD e retorno à cidade
- World Map Interaction: nós tipados (cidade/vilarejo/recurso/criatura/dragão/marco), marcha provisória com tempo de deslocamento, coleta de recursos, persistência City↔WorldMap e contrato `IHeroesGateway.TryReserveMarchSlot`
- Criaturas do World Map: `WorldCreatureDefinition`/`Instance`, `CreatureRewardTable`, `CreatureDifficultyResolver`, `CreatureEncounterService` (engajar, resolver provisório, respawn, energia)
- Marchas completas e ocupação de nós: `MarchStateMachine`, `MarchService`, `MarchRepository`, `MarchTravelCalculator`, `WorldNodeOccupationService`, `MarchChangedEvent`
- Coleta completa no mapa: taxa/`gatherRatePerHour`, carga da marcha, depleção e respawn de nós de recurso (`WorldResourceGatheringService`)
- Energia do mapa mundial: `PlayerEnergyWallet`, regen por timestamp, custos configuráveis, HUD com ETA e persistência dedicada (`EnergyPersistenceRepository`)
- Filtros, localizar e visão territorial no World Map: visibilidade combinável, foco de câmera com limites, overlays Neutral/Owned/Allied/Enemy/Contested/Locked
- Patch de restauração do World Map: câmera/zoom, seleção por ID, tick global de marchas e depósito de carteira atômico (`rewardDeliveryId`/`IsCommitted`)
- Documentação `docs/architecture/player-city.md` e `docs/architecture/world-map.md`

## [0.2.0] — 2026-07-26

### Added

- Sistema de heróis (Vortex + 10 heroínas) orientado a `docs/game-design/heroes/heroes.seed.json`
- Backend: catálogo, facções, vantagem circular (+15%), bônus 3/3+2/4/5, poderes READY/ACTIVE/COOLDOWN, roster do jogador, validação de equipe
- Endpoints `/api/heroes/*`, `/api/players/me/heroes`, `/api/teams/validate`, `/api/battle/.../special/activate`
- Migration EF `AddHeroesSystem` e seed automático do catálogo
- Unity: `Assets/Valgor/Heroes/` (Data, Factions, SpecialPowers, Magic, Skins, UI, Preview360, placeholders Addressables)
- Testes de domínio/aplicação para vantagem, bônus, cooldown, idempotência e nomes “A definir”

## [0.1.0] — 2026-07-25

### Added

- Monorepo oficial Valgor Studios (client, server, admin, database, infra, docs, assets, tools)
- Backend .NET 9 em Clean Architecture com MediatR 12.4.1, FluentValidation, Result Pattern, Domain Events e BaseEntity
- Autenticação JWT (`POST /api/auth/login`)
- Endpoints de sistema `GET /health` e `GET /version`
- Swagger, Serilog, HealthChecks (PostgreSQL, Redis, EF Core)
- Middleware global de exceções e Validation Pipeline
- Entity Framework Core + Npgsql com migration `InitialCreate` e seed de admin em Development
- Redis cache e Docker Compose (PostgreSQL 5437, Redis 6383, pgAdmin 5051)
- Admin React + Vite + TypeScript com login, dashboard, menu lateral, tema e auth JWT
- Client Unity 6 LTS com URP, Addressables, Input System, Localization, pooling, áudio, scene loader e loading screen
- GitHub Actions de Build e Test do backend

### Security

- Hash de senha PBKDF2 (SHA-256, 100k iterações)
- JWT com validação de issuer, audience e lifetime
- MediatR 12.4.1 (licença Apache-2.0) — sem dependência comercial
