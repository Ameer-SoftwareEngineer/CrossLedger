# CrossLedgerWeb

Cross-border multi-currency wallet & remittance platform — an ASP.NET Core Web API backend, a Blazor WebAssembly frontend, a SQL Server ledger, and a full Azure deployment.

CrossLedgerWeb is not a CRUD demo. It models the problems real payment infrastructure has to solve: a double-entry ledger where balances are derived rather than stored, FX quote locking, idempotent transfers, optimistic concurrency, multi-provider payment routing with audited failover, and step-up authentication on sensitive operations.

The full design rationale, business rules, data access strategy, provider orchestration, security model and delivery roadmap are documented in [`CrossLedger_Specification-2.pdf`](CrossLedger_Specification-2.pdf) — read that first.

## Solution structure

```
CrossLedgerWeb.sln
src/
  CrossLedgerWeb.Domain            entities, value objects, domain events — no external dependencies
  CrossLedgerWeb.Application       CQRS handlers, interfaces, validators — depends on Domain
  CrossLedgerWeb.Infrastructure    EF Core, Redis, Dapper, email — depends on Application, Domain
  CrossLedgerWeb.Providers         payment/FX provider implementations behind shared interfaces
  CrossLedgerWeb.Api               controllers, middleware, DI wiring — depends on Application, Infrastructure
  CrossLedgerWeb.Client            Blazor WebAssembly front end
  CrossLedgerWeb.Shared            DTOs and validators shared by Api + Client
tests/
  CrossLedgerWeb.Domain.Tests          pure unit tests, no mocks
  CrossLedgerWeb.Application.Tests     unit tests with Moq
  CrossLedgerWeb.Integration.Tests     WebApplicationFactory + Testcontainers
infra/
  docker-compose.yml            local API + SQL Server + Redis
  main.bicep                    Azure environment as code (added during cloud-deploy milestone)
```

Dependencies point inward only (Clean Architecture): `Domain` has no references, and outer layers depend on abstractions defined further in.

## Getting started

```bash
dotnet restore
dotnet build
dotnet test
```

Local infrastructure (SQL Server, Redis):

```bash
docker compose -f infra/docker-compose.yml up -d
```

## Status

Fresh scaffold — solution and project skeletons only. Build follows the week-by-week roadmap in section 10 of the specification, starting with the Domain layer and the double-entry ledger engine.

## Branching

`main` is the protected, always-buildable branch. Work happens on `feature/*` branches merged back via pull request.
