# Guilda dos Mensageiros — Microservices Messaging Architecture

A distributed messaging system built with **.NET 9 microservices**. The domain — a "Messenger
Guild" delivering messages — is a thin excuse for the real focus: **architecture, design
patterns, and asynchronous messaging** (hexagonal architecture, CQRS, event-driven design, DDD).

## Services

| Service | Role | Status |
|---|---|---|
| **DispatchService** (API) | HTTP entry point, creates messages, publishes events (Outbox pattern) | ✅ Complete |
| **DeliveryService** (worker) | Processes deliveries, simulates delivery attempts (retry, circuit breaker) | ✅ Complete |
| **InboxService** (worker + API) | Recipient's timeline (CQRS materialized view) | ✅ Complete |
| **NotificationService** (worker) | Sends notifications (strategy pattern, providers) | ✅ Complete |

Each service follows **hexagonal architecture** (Domain / Application / Infrastructure /
Integration layers) and communicates only via **RabbitMQ** (MassTransit) — fanout exchanges for
events, direct queues for commands, with DLQs and retry/backoff for reliability.

## Stack

.NET 10 · MassTransit + RabbitMQ · EF Core 9 + SQL Server · MediatR (CQRS) · Serilog ·
**.NET Aspire** (orchestration, service discovery, OpenTelemetry, dashboard) · Docker

## How to run

### With .NET Aspire (recommended)

Orchestrates all 5 services + RabbitMQ + SQL Server with one command, and opens the
**Aspire Dashboard** (live logs, distributed traces, metrics, resource graph) automatically.
Requires Docker Desktop running.

```bash
git clone https://github.com/rafael-ventura/guilda-mensageria-microservices.git
cd guilda-mensageria-microservices
dotnet run --project GuildaMensageria.AppHost
```

### Without Aspire (manual, docker-compose)

```bash
docker-compose up -d                                  # RabbitMQ + SQL Server

cd DispatchService/DispatchService.Host.Api
dotnet run
# ...and the other 4 Host projects in separate terminals
```

- Swagger (Dispatch): `https://localhost:7000/swagger`
- Inbox timeline: `GET /api/inbox/{destinatario}` on InboxService.Host.Api
- RabbitMQ management UI: `http://localhost:15672` (`admin` / `admin123`)

```bash
curl -X POST https://localhost:7000/api/recados \
  -H "Content-Type: application/json" \
  -d '{"remetente":"Alice","destinatario":"Bob","conteudo":"Hi!","enderecoEntrega":"123 Main St"}'
```

## Notes

All 4 services are implemented end to end today. See `ROADMAP.md` for the living plan of
what's done and what's next (observability polish, integration tests, a domain-data
dashboard). Full pattern-by-pattern breakdown of what's implemented where lives in the code
comments and project structure rather than duplicated here.
