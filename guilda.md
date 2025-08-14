Briefing do Projeto — Guilda dos Mensageiros (Microsserviços + Mensageria)
Contexto & Objetivo
Quero um projeto didático, porém realista, para treinar microsserviços com mensageria usando .NET. O domínio é lúdico (entregas de “recados” por uma guilda), mas o foco é arquitetura, padrões de projeto e boas práticas.

Stack técnica (fixa)
.NET 9 (Host genérico para workers e WebAPI para o gateway).

RabbitMQ + MassTransit (bus de mensagens).

MediatR (Commands/Queries/Notifications locais em cada serviço).

EF Core (ou Dapper) + Postgres (persistência) — Outbox quando aplicável.

Serilog (logs) e OpenTelemetry (telemetria, quando útil).

Docker Compose para infra local (RabbitMQ/Postgres).

Contracts como projeto separado para DTOs de integração (eventos/comandos entre serviços).

Importante: não invente libs novas sem justificar. Se sugerir algo fora da lista, explique por que, impacto e como remove se não servir.

Arquitetura (Hexagonal / Ports & Adapters)
Serviços:

DispatchService (único com API HTTP): recebe requisições REST, processa commands locais (MediatR), persiste e publica eventos de integração.

DeliveryService (Worker): consome eventos de criação, simula entrega, publica eventos de status e emite comandos de integração para notificação.

InboxService (Worker): mantém a “timeline” do destinatário; consome eventos (criação/entrega) e comandos de integração quando existirem.

NotificationService (Worker): consome comando de integração para notificar remetente/destinatário (simulado).

Padrões:

Command (local): intenção imperativa dentro do serviço (MediatR).

Event (integração): fato ocorrido entre serviços (MassTransit/RabbitMQ).

CQRS: separar leitura/escrita no nível de cada serviço (principalmente Inbox e Dispatch).

Idempotência nos handlers de mensagens.

Retry com backoff e DLQ (Dead Letter Queue) para falhas repetidas.

Outbox para consistência (quando publicar evento após transação).

Adapters (Ports & Adapters): inbound (Controllers/Consumers) e outbound (Repos/Publishers).

Topologia de Mensageria (nomes canônicos)
Exchanges (fanout)

recado.events — publica: DispatchService; consomem: DeliveryService, InboxService.

entrega.events — publica: DeliveryService; consome: InboxService (e Notification opcional).

Queues (direct/commands)

notificacao.commands — publica: DeliveryService; consome: NotificationService.

inbox.commands — publica: Dispatch/Delivery (quando preferir comando); consome: InboxService.

DLQ: sufixo *.dlq para cada fila com política de retry.

Regra: eventos são broadcast (fanout); comandos de integração vão para fila dedicada do serviço alvo.

Estrutura de Pastas por Serviço
Somente DispatchService tem API; os demais são Worker.

bash
Copiar
Editar
<Service>/
  <Service>.Host.Api     # só para o DispatchService
  <Service>.Host.Worker  # para Delivery/Inbox/Notification
  <Service>.Application  # Commands/Queries/Handlers (MediatR) – uso local
  <Service>.Domain       # Entidades/VOs/Domain Events (internos)
  <Service>.Infrastructure  # DB, Outbox, config do bus, providers
  <Service>.Integration     # MENSAGERIA (adapters)
    Topology/               # nomes de exchanges/filas, convenções
    EventsIn/               # Consumers (MassTransit) de eventos externos
    EventsOut/              # Publishers de eventos externos
    CommandsIn/             # Consumers de comandos de integração
    CommandsOut/            # Publishers de comandos de integração
    Mappings/               # mapeamento Domain ↔ Contracts (DTOs)
Projeto compartilhado Contracts/:

Apenas DTOs de integração: RecadoCriadoEvent, EntregaConcluidaEvent, EntregaFalhouEvent, EnviarNotificacaoCommand, RegistrarNaInboxCommand, etc.

Não colocar entidades de domínio aqui.

Fluxo ponta-a-ponta (sequência)
mermaid
Copiar
Editar
sequenceDiagram
  participant Cliente
  participant DispatchApi as DispatchService.Api
  participant DispatchApp as DispatchService.App
  participant Rabbit as RabbitMQ
  participant Delivery as DeliveryService.Worker
  participant Inbox as InboxService.Worker
  participant Notify as NotificationService.Worker

  Cliente->>DispatchApi: POST /recados (DTO)
  DispatchApi->>DispatchApp: MediatR.Send(CriarRecadoCommand)
  DispatchApp-->>DispatchApi: Id do recado

  DispatchApp->>Rabbit: Publish RecadoCriadoEvent (recado.events)
  Rabbit-->>Delivery: RecadoCriadoEvent
  Rabbit-->>Inbox: RecadoCriadoEvent

  Delivery->>Delivery: Tentar entrega (retry/backoff)
  alt sucesso
    Delivery->>Rabbit: Publish EntregaConcluidaEvent (entrega.events)
    Delivery->>Rabbit: Send EnviarNotificacaoCommand (notificacao.commands)
  else falha
    Delivery->>Rabbit: Publish EntregaFalhouEvent (entrega.events)
    Delivery->>Rabbit: Send EnviarNotificacaoCommand (notificacao.commands)
  end

  Inbox->>Inbox: Atualiza timeline (criação + status)
  Notify->>Notify: Envia notificação (simulada)
Convenções & Regras
Naming: recado.events, entrega.events, notificacao.commands, inbox.commands, DLQ *.dlq.

Versionamento de contratos: adotar versionamento por namespace ou sufixo (ex.: v1) e evitar breaking changes; quando quebrar, novo contrato (ex.: EntregaConcluidaEventV2).

Config (por serviço):

RABBITMQ_HOST, RABBITMQ_USER, RABBITMQ_PASS, RABBITMQ_VHOST

DB_CONNECTIONSTRING

PREFETCH_COUNT, RETRY_INTERVALS (segundos), DLQ_SUFFIX

Observabilidade: logs com Serilog; se usar OTel, exportar no console para dev.

O que construir primeiro (Iteração 1)
Solução e projetos com as pastas propostas.

docker-compose com RabbitMQ (management) e Postgres.

Bootstrap dos hosts:

Dispatch Api: DI de MediatR, leitura de configs, MassTransit conectado (sem domínio).

Delivery/Inbox/Notification Workers: DI e MassTransit prontos para consumir/publicar.

Declaração de topologia (nomes e bindings) e registro no bus (MassTransit).

Contracts v1: criar DTOs (vazios por enquanto ou com campos mínimos).

Políticas de retry/backoff e DLQ configuráveis por settings.

Sem implementar regras de domínio agora — a prioridade é infra + mensageria funcionando.

Critérios de Aceite (D.O.D. da Iteração 1)
Todos os serviços sobem localmente (Rider) e conectam ao RabbitMQ do docker.

Exchanges/filas existem com nomes corretos.

Logs mostram tentativas de conexão e bindings concluídos.

Consumers registrados sem exceções.

Documento curto README.md na raiz com:

Como subir o docker, rodar cada serviço e URLs/porta.

Desenho/mermaid do fluxo (pode reaproveitar o acima).

Lista de contratos v1 e quem publica/consome.

Transparência exigida do Cursor
Explique toda decisão arquitetural e de biblioteca antes de gerar qualquer código/config.

Liste suposições que estiver fazendo; se faltar informação, pergunte.

Não gere trechos grandes de código de domínio agora. Foque em configuração e bootstrap.

Não adicione dependências além das citadas sem justificar custo/benefício e como revertê-las.

Ao propor mudanças na topologia ou nomes, mostre impacto (quem publica/consome, retrocompatibilidade).

Forneça passo a passo para testar localmente (incluindo comandos docker e como ver as filas no management UI).

Se algo falhar, reporte o erro observado, hipóteses de causa e próximos passos.

Entregáveis esperados do Cursor nesta etapa
Lista de projetos/targets a criar (sem lógica de domínio).

docker-compose.yml para RabbitMQ/Postgres.

appsettings base por serviço com chaves de Rabbit/DB.

Registro do MassTransit (bus) com exchanges/queues/bindings e políticas de retry/DLQ.

README com instruções de uso e diagrama do fluxo.

