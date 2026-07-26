# API

## `GET /health`

Liveness da aplicação (sem dependências externas).

```json
{
  "status": "ok",
  "version": "0.1.0"
}
```

## `GET /version`

```json
{
  "version": "0.1.0",
  "product": "Valgor",
  "environment": "Development",
  "serverTimeUtc": "2026-07-26T00:00:00Z"
}
```

## `GET /health/ready`

Readiness com checks de PostgreSQL, Redis e EF Core.

## `POST /api/auth/login`

```json
{
  "email": "admin@valgor.local",
  "password": "Valgor@Admin1"
}
```

Resposta:

```json
{
  "accessToken": "<jwt>",
  "tokenType": "Bearer",
  "email": "admin@valgor.local",
  "displayName": "Valgor Admin",
  "role": "Admin"
}
```
