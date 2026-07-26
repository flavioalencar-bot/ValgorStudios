# Arquitetura Valgor

## Princípios

1. Fundação de produção — sem protótipos descartáveis
2. Clean Architecture no backend
3. Contratos explícitos entre camadas e clientes
4. Infraestrutura local reproduzível via Docker
5. Observabilidade desde o dia zero (Serilog + HealthChecks)
6. SOLID · DRY · KISS · YAGNI

## Camadas backend

- **Api**: HTTP, JWT, Swagger, middleware de exceções
- **Application**: casos de uso (MediatR), FluentValidation pipeline, Result Pattern
- **Domain**: BaseEntity, AggregateRoot, Domain Events
- **Infrastructure**: EF Core / PostgreSQL / Redis / JWT / seed
- **Contracts**: DTOs compartilhados
- **Workers**: jobs assíncronos

## Client Unity

Bootstrap → LoadingFlow → MainMenu, com URP, Addressables, Input System, Localization, pooling e áudio.

Detalhes: [game-core.md](game-core.md) · [player-city.md](player-city.md) · [world-map.md](world-map.md) · [dragons.md](dragons.md)

### Game Core

- `ServiceRegistry`, `GameSession`, `GameStateMachine`
- `SceneLoader`, `LoadingFlow`, `GameNavigator`
- Contratos de módulo: cidade, world map, buildings, resources, dragons, `IHeroesGateway`

### Player City

- 14 edifícios provisórios, recursos, seleção, upgrade, câmera isométrica, HUD
- Controles: clique seleciona · direito/meio pan · scroll/pinça zoom

## Admin

React + Vite com autenticação JWT, layout com menu lateral e dashboard.
