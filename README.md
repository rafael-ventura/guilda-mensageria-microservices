# Guilda dos Mensageiros — Microservices Messaging Architecture

A distributed messaging system built with **.NET 9 microservices**. The domain — a "Messenger
Guild" delivering messages — is a thin excuse for the real focus: **architecture, design
patterns, and asynchronous messaging** (hexagonal architecture, CQRS, event-driven design, DDD).

## Services

| Service | Role | Status |
|---|---|---|
| **DispatchService** (API) | HTTP entry point, creates messages, publishes events (Outbox pattern) | ✅ Complete |
| **DeliveryService** (worker) | Processes deliveries, simulates delivery attempts (saga, retry, circuit breaker) | 🔲 Planned |
| **InboxService** (worker) | Recipient's timeline/history (CQRS, event sourcing) | 🔲 Planned |
| **NotificationService** (worker) | Sends notifications (strategy pattern, providers) | 🔲 Planned |

Each service follows **hexagonal architecture** (Domain / Application / Infrastructure /
Integration layers) and communicates only via **RabbitMQ** (MassTransit) — fanout exchanges for
events, direct queues for commands, with DLQs and retry/backoff for reliability.

## Stack

.NET 9 · MassTransit + RabbitMQ · EF Core 9 + SQL Server · MediatR (CQRS) · Serilog · Docker Compose

## How to run

```bash
git clone https://github.com/rafael-ventura/guilda-mensageria-microservices.git
cd guilda-mensageria-microservices
docker-compose up -d                                  # RabbitMQ + SQL Server

cd DispatchService/DispatchService.Host.Api
dotnet run
```

- Swagger: `https://localhost:7000/swagger`
- RabbitMQ management UI: `http://localhost:15672` (`admin` / `admin123`)

```bash
curl -X POST https://localhost:7000/api/recados \
  -H "Content-Type: application/json" \
  -d '{"remetente":"Alice","destinatario":"Bob","conteudo":"Hi!","enderecoEntrega":"123 Main St"}'
```

## Notes

Only DispatchService is fully implemented end to end today; the three worker services are
scaffolded (topology, consumers, retry/DLQ policies configured) but their business logic is
still pending. Full pattern-by-pattern breakdown of what's implemented where lives in the code
comments and project structure rather than duplicated here.
