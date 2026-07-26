# API

## `GET /health`

Resposta de liveness da aplicação (sem dependências externas).

```json
{
  "status": "ok",
  "version": "0.1.0"
}
```

## `GET /health/ready`

Readiness com checks de PostgreSQL, Redis e EF Core.
