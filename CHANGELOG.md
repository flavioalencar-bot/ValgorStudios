# Changelog

Todas as mudanças relevantes deste repositório são documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/),
e o projeto adere a [SemVer](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Added

- Vortex jogável: rig Humanoid no Blender, 16 clips mínimos, `Vortex_DragonSword.fbx`, Avatar Humanoid no Unity, espada em `Socket_RightHand`, VFX Domínio do Rei (~10s) e preview HeroesDemo sem dummy
- Script de produção `production/Vortex/rig_animate_weapon_vortex.py` (skinning + animações + export FBX)

### Added

- Dragon Foundation em `Assets/Valgor/Dragons`: definição/instância, state machine, ninho, alimentação, recuperação, deployment em marchas e `IDragonGateway`
- Integração City (Torre dos Dragões) + World Map (destaque/recall/poder provisório) + recursos (Food/Essence)
- Documentação `docs/architecture/dragons.md`
- Complemento 02 de dragões: estados oficiais (EGG/JUVENILE/DEPLOYED/…), `DragonHungerService`, descanso e recuperação com timers
- Complemento 03 de dragões: estágios de crescimento (HATCHLING…ANCIENT), vínculo e evolução de espécie

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
