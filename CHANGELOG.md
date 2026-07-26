# Changelog

Todas as mudanças relevantes deste repositório são documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/),
e o projeto adere a [SemVer](https://semver.org/lang/pt-BR/).

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
