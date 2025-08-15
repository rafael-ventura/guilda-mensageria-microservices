# Guilda dos Mensageiros - Arquitetura de Microsserviços

## 📋 Visão Geral

Este projeto implementa um sistema de mensageria distribuída usando **microsserviços** com **.NET 9**, demonstrando padrões arquiteturais modernos e boas práticas para sistemas distribuídos. O domínio simula uma "Guilda de Mensageiros" que entrega recados, mas o foco principal é a **arquitetura, padrões de projeto e mensageria assíncrona**.

## 🏗️ Arquitetura Geral

### Padrões Arquiteturais Aplicados

#### 🔵 **Arquitetura Hexagonal (Ports & Adapters)**
Cada microsserviço segue a **Arquitetura Hexagonal**, garantindo:
- **Isolamento do domínio** das preocupações de infraestrutura
- **Testabilidade** através de interfaces bem definidas
- **Flexibilidade** para trocar implementações sem afetar a lógica de negócio

#### 🔵 **CQRS (Command Query Responsibility Segregation)**
- **Commands**: Operações de escrita usando MediatR
- **Queries**: Operações de leitura otimizadas
- **Separação clara** entre leitura e escrita, especialmente nos serviços Dispatch e Inbox

#### 🔵 **Event-Driven Architecture**
- **Comunicação assíncrona** entre microsserviços via eventos
- **Desacoplamento temporal** - serviços podem processar eventos independentemente
- **Resiliência** através de filas e retry policies

#### 🔵 **Domain-Driven Design (DDD)**
- **Bounded Contexts** bem definidos por microsserviço
- **Aggregates** e **Value Objects** no layer Domain
- **Domain Events** para comunicação interna

## 🎯 Microsserviços

### 🌐 **DispatchService** (API Gateway)
**Responsabilidade**: Ponto de entrada HTTP para criação de recados

```
DispatchService/
├── DispatchService.Host.Api/          # 🌐 Web API + Swagger
├── DispatchService.Application/       # 📋 Commands/Queries (MediatR)
├── DispatchService.Domain/           # 🏛️ Entidades e Regras de Negócio
├── DispatchService.Infrastructure/   # 🗄️ EF Core + Postgres + Outbox
└── DispatchService.Integration/      # 📡 MassTransit + RabbitMQ
    ├── Topology/                     # 🏗️ Definições de Exchanges/Queues
    ├── EventsOut/                    # 📤 Publishers de Eventos
    ├── CommandsOut/                  # 📤 Publishers de Comandos
    └── Mappings/                     # 🔄 Domain ↔ Contracts
```

**Padrões Aplicados**:
- **API Gateway Pattern**: Ponto único de entrada
- **Outbox Pattern**: Consistência entre persistência e publicação de eventos
- **Command Pattern**: Operações encapsuladas via MediatR

---

### 🚚 **DeliveryService** (Worker)
**Responsabilidade**: Processa entregas e simula tentativas de entrega

```
DeliveryService/
├── DeliveryService.Host.Worker/      # ⚙️ Background Service
├── DeliveryService.Application/      # 📋 Handlers de Eventos
├── DeliveryService.Domain/          # 🏛️ Lógica de Entrega
├── DeliveryService.Infrastructure/  # 🗄️ Persistência de Estado
└── DeliveryService.Integration/     # 📡 Consumers + Publishers
    ├── EventsIn/                    # 📥 Consome RecadoCriadoEvent
    ├── EventsOut/                   # 📤 Publica EntregaConcluidaEvent/FalhouEvent
    └── CommandsOut/                 # 📤 Envia EnviarNotificacaoCommand
```

**Padrões Aplicados**:
- **Saga Pattern**: Coordenação de processo de entrega
- **Retry Pattern**: Tentativas com backoff exponencial
- **Circuit Breaker**: Proteção contra falhas em cascata

---

### 📥 **InboxService** (Worker)
**Responsabilidade**: Mantém timeline/histórico do destinatário

```
InboxService/
├── InboxService.Host.Worker/        # ⚙️ Background Service  
├── InboxService.Application/        # 📋 CQRS Handlers
├── InboxService.Domain/            # 🏛️ Timeline/Inbox Domain
├── InboxService.Infrastructure/    # 🗄️ Read/Write Models
└── InboxService.Integration/       # 📡 Event Consumers
    ├── EventsIn/                   # 📥 Todos os eventos do sistema
    ├── CommandsIn/                 # 📥 RegistrarNaInboxCommand
    └── Mappings/                   # 🔄 Event ↔ Timeline Entry
```

**Padrões Aplicados**:
- **CQRS**: Separação clara entre escrita (eventos) e leitura (timeline)
- **Event Sourcing**: Timeline construída a partir de eventos
- **Materialized View**: Timeline otimizada para consultas

---

### 🔔 **NotificationService** (Worker)
**Responsabilidade**: Envia notificações para remetentes e destinatários

```
NotificationService/
├── NotificationService.Host.Worker/     # ⚙️ Background Service
├── NotificationService.Application/     # 📋 Notification Handlers  
├── NotificationService.Domain/         # 🏛️ Notification Logic
├── NotificationService.Infrastructure/ # 🗄️ Providers (Email, SMS, etc)
└── NotificationService.Integration/    # 📡 Command Consumers
    ├── CommandsIn/                     # 📥 EnviarNotificacaoCommand
    └── Mappings/                       # 🔄 Command ↔ Notification
```

**Padrões Aplicados**:
- **Strategy Pattern**: Diferentes tipos de notificação (Email, SMS, Push)
- **Template Method**: Estrutura comum para envio de notificações
- **Adapter Pattern**: Integração com provedores externos

## 📦 Projeto Contracts

```
Contracts/
├── Events/                          # 📡 Eventos de Integração
│   ├── RecadoCriadoEvent.cs
│   ├── EntregaConcluidaEvent.cs
│   └── EntregaFalhouEvent.cs
└── Commands/                        # 📋 Comandos de Integração
    ├── EnviarNotificacaoCommand.cs
    └── RegistrarNaInboxCommand.cs
```

**Princípios Aplicados**:
- **Shared Kernel**: DTOs compartilhados entre bounded contexts
- **Backward Compatibility**: Versionamento cuidadoso dos contratos
- **Schema Evolution**: Preparado para evolução sem quebrar integrações

## 🔄 Fluxo de Mensageria

### Topologia RabbitMQ

```mermaid
graph TD
    A[Cliente] -->|POST /recados| B[DispatchService.Api]
    B -->|MediatR Command| C[DispatchService.App]
    C -->|Publish| D[recado.events Exchange]
    
    D -->|Fanout| E[DeliveryService Queue]
    D -->|Fanout| F[InboxService Queue]
    
    E -->|Process| G[DeliveryService]
    G -->|Success| H[entrega.events Exchange]
    G -->|Command| I[notificacao.commands Queue]
    
    H -->|Fanout| J[InboxService Queue]
    I -->|Direct| K[NotificationService]
    
    F -->|Process| L[InboxService]
    J -->|Process| L
```

### Padrões de Mensageria

#### 🔀 **Publish-Subscribe (Events)**
- **Exchanges Fanout**: `recado.events`, `entrega.events`
- **Múltiplos Consumers**: Cada serviço interessado recebe uma cópia
- **Desacoplamento**: Publishers não conhecem subscribers

#### 📬 **Point-to-Point (Commands)**  
- **Queues Direct**: `notificacao.commands`, `inbox.commands`
- **Single Consumer**: Apenas o serviço alvo processa
- **Garantia de Entrega**: Cada comando é processado exatamente uma vez

#### 🔄 **Reliability Patterns**
- **Dead Letter Queues (DLQ)**: `*.dlq` para mensagens falhadas
- **Retry com Backoff**: Tentativas exponenciais
- **Idempotência**: Handlers seguros para reprocessamento

## 🛠️ Stack Técnica

### Core Framework
- **.NET 9**: Host genérico + WebAPI
- **C# 13**: Records, Pattern Matching, Global Usings

### Mensageria
- **RabbitMQ**: Message Broker
- **MassTransit**: Abstração para mensageria + Patterns

### Persistência  
- **SQL Server**: Banco principal (migrado do PostgreSQL)
- **Entity Framework Core 9**: ORM + Migrations + Configurations
- **Repository Pattern**: Abstração de acesso a dados
- **Unit of Work**: Coordenação de transações
- **Outbox Pattern**: Consistência entre persistência e mensageria

### Observabilidade
- **Serilog**: Logging estruturado
- **OpenTelemetry**: Distributed Tracing (futuro)

### Orquestração
- **Docker Compose**: Infraestrutura local
- **Background Services**: Workers .NET

## 🧪 Testes e Qualidade

### Estrutura de Testes (Planejada)
```
Tests/
├── Unit/                           # 🔬 Testes Unitários
│   ├── Domain/                     # Regras de negócio
│   └── Application/                # Handlers MediatR
├── Integration/                    # 🔗 Testes de Integração  
│   ├── API/                        # Endpoints
│   └── Messaging/                  # Publishers/Consumers
└── E2E/                           # 🎭 Testes End-to-End
    └── Scenarios/                  # Fluxos completos
```

### Padrões de Teste
- **AAA Pattern**: Arrange, Act, Assert
- **Test Doubles**: Mocks, Stubs, Fakes
- **Integration Testing**: TestContainers para deps externas
- **Contract Testing**: Pact para APIs

## 🚀 Como Executar

### Pré-requisitos
- .NET 9 SDK
- Docker Desktop
- PowerShell (para scripts)

### Passos

1. **Clone o repositório**
```bash
git clone https://github.com/rafael-ventura/guilda-mensageria-microservices.git
cd guilda-mensageria-microservices
```

2. **Teste todos os builds**
```powershell
.\test-all-builds.ps1
```

3. **Subir infraestrutura** (próximo passo)
```bash
docker-compose up -d
```

4. **Executar serviços** (próximo passo)
```bash
# DispatchService (API)
cd DispatchService/DispatchService.Host.Api
dotnet run

# Workers (em terminais separados)
cd DeliveryService/DeliveryService.Host.Worker
dotnet run

cd InboxService/InboxService.Host.Worker  
dotnet run

cd NotificationService/NotificationService.Host.Worker
dotnet run
```

## 📚 Status de Implementação

### ✅ Iteração 1: Estrutura Base (CONCLUÍDA)
- [x] Solução e projetos com estrutura hexagonal
- [x] Docker Compose (RabbitMQ + SQL Server)
- [x] Bootstrap MassTransit + MediatR em todos os serviços
- [x] Configuração de topologia de mensageria
- [x] Políticas de retry/DLQ configuráveis
- [x] Consumers básicos implementados

### ✅ DispatchService (COMPLETO)
- [x] **Domain Layer**: Entidades (Recado, OutboxMessage) + Repository interfaces
- [x] **Infrastructure Layer**: EF Core + SQL Server + Repository implementations
- [x] **Application Layer**: Commands/Handlers + Outbox Pattern
- [x] **API Layer**: REST Controller + Swagger + Validações
- [x] **Database**: Migrações EF Core + Configurações de entidades
- [x] **Patterns**: Unit of Work + Repository + CQRS + Outbox

### 🔄 Iteração 2: Microsserviços Restantes
- [ ] **DeliveryService**: Domain + Consumers + Publishers + Saga Pattern
- [ ] **InboxService**: CQRS + Timeline + Event Sourcing + Materialized Views  
- [ ] **NotificationService**: Strategy Pattern + Providers + Templates
- [ ] **Testes de Integração**: Fluxo end-to-end completo

### 🔄 Iteração 3: Observabilidade & Qualidade
- [ ] OpenTelemetry + Jaeger (Distributed Tracing)
- [ ] Health Checks em todos os serviços
- [ ] Métricas customizadas (Prometheus)
- [ ] Dashboard Grafana
- [ ] Testes unitários e de integração
- [ ] Contract Testing (Pact)

### 🔄 Iteração 4: Produção & DevOps
- [ ] CI/CD Pipeline
- [ ] Kubernetes manifests
- [ ] Helm Charts
- [ ] Monitoring & Alerting
- [ ] Performance Testing

## 🎯 Objetivos de Aprendizado

Este projeto demonstra:

✅ **Arquitetura de Microsserviços** com separação clara de responsabilidades
✅ **Event-Driven Architecture** com mensageria assíncrona
✅ **Domain-Driven Design** com bounded contexts bem definidos  
✅ **CQRS** para separação de leitura e escrita
✅ **Hexagonal Architecture** para testabilidade e flexibilidade
✅ **Reliability Patterns** para sistemas distribuídos resilientes
✅ **Modern .NET** com as melhores práticas da plataforma

---

## 🚀 Como Testar o DispatchService

### 1. **Subir Infraestrutura**
```bash
docker-compose up -d
```

### 2. **Executar DispatchService**
```bash
cd DispatchService/DispatchService.Host.Api
dotnet run
```

### 3. **Testar API**
- **Swagger UI**: `https://localhost:7000/swagger`
- **Endpoint**: `POST https://localhost:7000/api/recados`

**Exemplo de Request:**
```json
{
  "remetente": "Alice",
  "destinatario": "Bob", 
  "conteudo": "Olá! Como você está?",
  "enderecoEntrega": "Rua das Flores, 123"
}
```

### 4. **Verificar RabbitMQ**
- **Management UI**: `http://localhost:15672`
- **Usuário**: `admin` / **Senha**: `admin123`
- **Verificar**: Exchanges `recado.events` criado automaticamente

---

## 📊 Status Atual do Projeto

### ✅ **CONCLUÍDO (100%)**
- **Infraestrutura**: Docker Compose + RabbitMQ + SQL Server
- **Mensageria**: MassTransit + Topologia + Consumers básicos  
- **DispatchService**: Completo com todos os layers + Outbox Pattern
- **Padrões**: Repository + Unit of Work + CQRS + Domain-Driven Design

### 🔄 **PRÓXIMOS PASSOS**
1. **DeliveryService** - Implementar processamento de entregas
2. **InboxService** - Implementar timeline do destinatário
3. **NotificationService** - Implementar envio de notificações
4. **Teste End-to-End** - Fluxo completo funcionando

---

**🎉 DispatchService está 100% funcional! Pronto para processar recados e publicar eventos!** 🚀
