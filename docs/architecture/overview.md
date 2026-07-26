# Arquitetura Valgor

## Princípios

1. Fundação de produção — sem protótipos descartáveis
2. Clean Architecture no backend
3. Contratos explícitos entre camadas e clientes
4. Infraestrutura local reproduzível via Docker
5. Observabilidade desde o dia zero (Serilog + HealthChecks)

## Camadas backend

- **Api**: entrada HTTP
- **Application**: orquestração de casos de uso (MediatR + FluentValidation)
- **Domain**: núcleo de regras (ainda sem lógica de negócio nesta fundação)
- **Infrastructure**: EF Core / PostgreSQL / Redis
- **Contracts**: DTOs compartilhados
- **Workers**: jobs e processamento assíncrono
