# Valgor Studios

Plataforma oficial do ecossistema **Valgor** — fundação de produção para cliente Unity, backend .NET, painel administrativo e infraestrutura containerizada.

> Versão: `0.1.0` · Sprint 001 — Foundation  
> Repositório: [flavioalencar-bot/ValgorStudios](https://github.com/flavioalencar-bot/ValgorStudios)

---

## Visão

A Valgor Studios desenvolve experiências interativas com arquitetura escalável e preparada para produção desde o primeiro commit. Este monorepo é a base definitiva do produto — sem protótipos descartáveis.

---

## Arquitetura

```
┌─────────────┐     ┌─────────────┐     ┌──────────────────┐
│ Unity 6 LTS │────▶│  Valgor.Api │────▶│ PostgreSQL       │
│   client/   │     │   (.NET 9)  │     │ Redis            │
└─────────────┘     └──────┬──────┘     └──────────────────┘
                           │
┌─────────────┐            │            ┌──────────────────┐
│ Admin React │────────────┘            │ Valgor.Workers   │
│   admin/    │                         │ (background)     │
└─────────────┘                         └──────────────────┘
```

### Backend (Clean Architecture)

| Projeto | Responsabilidade |
|---------|------------------|
| `Valgor.Api` | HTTP, JWT, Swagger, HealthChecks, Serilog, exceptions |
| `Valgor.Application` | Casos de uso, MediatR, FluentValidation, Result |
| `Valgor.Domain` | BaseEntity, Domain Events, Aggregates |
| `Valgor.Infrastructure` | EF Core, PostgreSQL, Redis, JWT, seed |
| `Valgor.Contracts` | DTOs compartilhados |
| `Valgor.Workers` | Host de background jobs |

---

## Stack

| Camada | Tecnologia |
|--------|------------|
| Client | Unity 6 LTS · URP · Addressables · UI Toolkit · Input System · Localization |
| Backend | .NET 9 · EF Core · MediatR · FluentValidation · Serilog · Swagger · JWT |
| Banco | PostgreSQL 16 |
| Cache | Redis 7 |
| Admin | React · Vite · TypeScript |
| Containers | Docker Compose |
| CI | GitHub Actions |

---

## Estrutura

```
/
├── client/          # Unity 6 LTS
├── server/          # Solução .NET 9 (Valgor.sln)
├── admin/           # Painel React + Vite
├── database/        # Init SQL e seeds documentais
├── infra/           # Artefatos de infra
├── docs/            # Arquitetura e API
├── assets/          # Branding
├── tools/           # Scripts
├── docker-compose.yml
├── CHANGELOG.md
└── README.md
```

---

## Como executar

### Pré-requisitos

- .NET 9 SDK
- Docker Desktop
- Node.js 20+
- Unity 6 LTS (Hub)

### Portas reservadas

| Serviço | Porta |
|---------|-------|
| PostgreSQL | `5437` |
| Redis | `6383` |
| pgAdmin | `5051` |
| API | `5100` |
| Admin | `5173` |

### 1. Infraestrutura

```bash
docker compose up -d
```

- Postgres: `localhost:5437` · `valgor` / `valgor` / `valgor`
- Redis: `localhost:6383`
- pgAdmin: http://localhost:5051 · `admin@valgor.com` / `valgor`

### 2. Backend

```bash
cd server
dotnet restore
dotnet build
dotnet run --project Valgor.Api
```

Em Development a API aplica migrations e cria o admin inicial:

- email: `admin@valgor.local`
- senha: `Valgor@Admin1`

Endpoints:

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/health` | Liveness |
| GET | `/version` | Versão e ambiente |
| GET | `/health/ready` | Readiness (Postgres, Redis, EF) |
| POST | `/api/auth/login` | Autenticação JWT |
| GET | `/swagger` | OpenAPI |

### 3. Testes

```bash
cd server
dotnet test
```

### 4. Admin

```bash
cd admin
npm install
npm run dev
```

Abra http://localhost:5173 e autentique com o admin seed.

### 5. Client (Unity)

1. Instale **Unity 6 LTS** via Hub  
2. Abra a pasta `client/`  
3. Cenas: `Bootstrap` → `Loading` → `MainMenu`  
4. Pacotes: URP, Addressables, Input System, Localization, UI Toolkit

---

## CI

- **Build Backend** — restore, build, publish  
- **Test Backend** — suíte automatizada

---

## Licença

[MIT](LICENSE)
