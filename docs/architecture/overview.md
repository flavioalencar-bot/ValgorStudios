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

Bootstrap → Loading → MainMenu, com URP, Addressables, Input System, Localization, pooling e áudio.

## Admin

React + Vite com autenticação JWT, layout com menu lateral e dashboard.
