# Valgor Studios

Plataforma oficial do ecossistema **Valgor** — fundação de produção para cliente Unity, backend .NET, painel administrativo e infraestrutura containerizada.

> Versão da fundação: `0.1.0`  
> Repositório: [flavioalencar-bot/ValgorStudios](https://github.com/flavioalencar-bot/ValgorStudios)

---

## Visão do projeto

A Valgor Studios desenvolve experiências interativas e serviços digitais com arquitetura escalável, observável e preparada para produção desde o primeiro commit.

Este repositório é o **monorepo oficial**: concentra o cliente de jogo, a API, workers, contratos, painel admin, banco, infraestrutura e documentação — sem protótipos descartáveis.

Objetivos da fundação:

- Separação clara de responsabilidades (Clean Architecture no backend)
- Ambiente local reproduzível via Docker
- CI para build e testes do backend
- Contratos estáveis entre camadas e clientes
- Evolução incremental sem reescrever a base

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
| `Valgor.Api` | HTTP, Swagger, HealthChecks, Serilog |
| `Valgor.Application` | Casos de uso, MediatR, FluentValidation |
| `Valgor.Domain` | Entidades e regras de domínio |
| `Valgor.Infrastructure` | EF Core, Npgsql, Redis |
| `Valgor.Contracts` | DTOs e contratos compartilhados |
| `Valgor.Workers` | Processamento em background |

Fluxo de dependências: **Api / Workers → Application → Domain** · **Infrastructure → Application** · contratos em `Valgor.Contracts`.

---

## Stack

| Camada | Tecnologia |
|--------|------------|
| Client | Unity 6 LTS |
| Backend | .NET 9 |
| Banco | PostgreSQL 16 |
| Cache | Redis 7 |
| Admin | React + Vite |
| Containers | Docker / Docker Compose |
| CI | GitHub Actions |

Pacotes backend já integrados: **EF Core**, **Npgsql**, **Redis**, **Swagger**, **HealthChecks**, **Serilog**, **FluentValidation**, **MediatR**.

---

## Estrutura

```
/
├── client/          # Unity 6 LTS
├── server/          # Solução .NET 9 (Valgor.sln)
├── admin/           # Painel React + Vite
├── database/        # Init SQL, migrations, seeds
├── infra/           # Nginx e artefatos de infra
├── docs/            # Arquitetura e API
├── assets/          # Branding e assets compartilhados
├── tools/           # Scripts utilitários
├── docker-compose.yml
└── README.md
```

---

## Como executar

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 20+](https://nodejs.org/) (admin)
- Unity 6 LTS (client)

### 1. Infraestrutura local

Portas reservadas para o Valgor (evitam conflito com outros projetos no host):

| Serviço | Porta host |
|---------|------------|
| PostgreSQL | `5437` |
| Redis | `6383` |
| pgAdmin | `5051` |
| API (dev) | `5100` |

```bash
docker compose up -d
```

- PostgreSQL: `localhost:5437` · db/user/pass: `valgor`
- Redis: `localhost:6383`
- pgAdmin: http://localhost:5051 · `admin@valgor.local` / `valgor`

### 2. Backend

```bash
cd server
dotnet restore
dotnet build
dotnet run --project Valgor.Api
```

Endpoints:

- `GET /health` → `{ "status": "ok", "version": "0.1.0" }`
- `GET /health/ready` → health checks (Postgres, Redis, EF)
- Swagger (Development): http://localhost:5100/swagger

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

### 5. Client (Unity)

Abra a pasta `client/` no Unity Hub com **Unity 6 LTS**.

---

## CI

Workflows em `.github/workflows/`:

- **Build Backend** — restore + build + publish da API
- **Test Backend** — execução da suíte de testes

---

## Licença

Distribuído sob a licença [MIT](LICENSE).
