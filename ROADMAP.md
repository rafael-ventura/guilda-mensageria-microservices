# Roadmap — Guilda dos Mensageiros

Painel visual (snapshot, republicar quando o progresso mudar bastante):
https://claude.ai/code/artifact/325062e3-936f-4bde-918d-daa12be92a84

Documento vivo. A ideia é avançar um pouco a cada sessão: escolher o próximo item não
marcado, implementar, marcar `[x]`, e registrar uma linha no **Log de Progresso** no fim
do arquivo. Qualquer sessão futura (sua ou do Claude) deve conseguir ler este arquivo e
saber exatamente onde paramos e por quê.

## Estado atual (2026-09-04)

| Serviço | Domain | Infrastructure | Application | Integration | Status real |
|---|---|---|---|---|---|
| DispatchService (API) | ✅ | ✅ | ✅ | ✅ | Completo, funcional ponta-a-ponta na saída |
| DeliveryService (worker) | 🔲 | 🔲 (TODO no Program.cs) | 🔲 | Consumer só loga + `Task.Delay` | Esqueleto |
| InboxService (worker) | 🔲 | 🔲 | 🔲 | **Consumer vazio** (arquivo existe sem código) | Não iniciado |
| NotificationService (worker) | 🔲 | 🔲 (TODO no Program.cs) | 🔲 | Consumer só loga + simula envio | Esqueleto |

Achado de limpeza: `NotificationService/NotificationService.{Application,Domain,Infrastructure,Integration}`
têm `.csproj` órfãos com nome `InboxService.*.csproj` (sobra de copy-paste, não referenciados
na `.sln`, mas confundem quem abrir a pasta).

---

## Fase 0 — Faxina rápida (< 1h)

- [x] Remover os `.csproj` órfãos `InboxService.*.csproj` dentro das pastas `NotificationService.*`
      (inclusive um extra em `NotificationService.Host.Worker`, achado depois)
- [x] **Achado de ambiente:** a máquina só tinha runtime .NET 6/7/8/10 instalado (sem o
      .NET 9), mas todos os projetos miravam `net9.0` — nada rodava (só compilava).
      Solução: subiu-se todo o solution (21 `.csproj`) para `net10.0`, alinhado ao SDK
      instalado e ao Aspire (que também é dessa geração)
- [x] **Achado de topologia:** `MessagingTopology.cs` documenta nomes canônicos
      (`recado.events`, `notificacao.commands`, etc.) mas o código nunca aplicava isso de
      fato — `cfg.ConfigureEndpoints(context)` usa a convenção default do MassTransit
      (nome por tipo de mensagem). Funciona (Publish/Consume batem sozinhos por
      convenção), só os nomes reais de exchange/fila no RabbitMQ não são os bonitinhos
      documentados. Por isso os novos consumers usam `IPublishEndpoint.Publish` também
      para os "comandos" (em vez de `Send` para uma fila com nome hardcoded, que
      quebraria). Alinhar os nomes reais via `SetEntityName`/`ReceiveEndpoint` explícito
      é um polimento futuro, não bloqueia nada hoje.
- [ ] *(opcional, não bloqueia o resto)* `Directory.Build.props` centralizando
      `TargetFramework net9.0`/`Nullable`/`ImplicitUsings`
- [ ] *(opcional, não bloqueia o resto)* `Directory.Packages.props` (central package
      management) para não ter versão do MassTransit/MediatR/EF Core divergente entre serviços

## Fase 1 — .NET Aspire (a espinha dorsal) ✅ concluída (2026-09-04)

Isso substitui o `docker-compose.yml` manual e já entrega boa parte do "painel" de graça:
o **Aspire Dashboard** mostra logs, traces distribuídos, métricas e o grafo de recursos
de todos os serviços rodando juntos, ao vivo.

- [x] `GuildaMensageria.AppHost` (Aspire 13.5.3, `dotnet new aspire-apphost`)
- [x] `GuildaMensageria.ServiceDefaults` (`dotnet new aspire-servicedefaults`)
- [x] `ServiceDefaults` referenciado nos 5 Host projects (Dispatch API, Delivery Worker,
      Inbox Worker, Inbox API, Notification Worker) — `builder.AddServiceDefaults()` em
      todos, `app.MapDefaultEndpoints()` nos dois Web (`/health`, `/alive`)
- [x] RabbitMQ (`Aspire.Hosting.RabbitMQ`, com management plugin) e SQL Server
      (`Aspire.Hosting.SqlServer`, 3 databases: GuildaDispatch/GuildaDelivery/GuildaInbox)
      modelados como recursos do AppHost, referenciados via `WithReference` +
      `WaitFor` em cada serviço
- [x] Cada `Program.cs` prefere a connection string injetada pelo Aspire
      (`GetConnectionString("rabbitmq")` / `GetConnectionString("Guilda{Service}")`) e
      cai para o `appsettings.json` manual quando ausente — dá pra rodar com ou sem
      Aspire, docker-compose continua funcionando como alternativa
- [x] Solution inteira compila (24 projetos, 0 warnings/0 erros) com o AppHost incluso
- [ ] **Pendente de validação em runtime:** Docker Desktop não estava rodando nesta
      máquina durante a implementação — rodar `dotnet run --project
      GuildaMensageria.AppHost` (ou `aspire run`) com Docker ligado e confirmar que os
      5 serviços sobem juntos e o Dashboard abre no browser mostrando tudo saudável

## Fase 2 — Terminar a lógica de negócio (o que realmente falta)

### DispatchService — corrigir lacuna crítica
- [ ] **Achado:** não existe nenhum `BackgroundService`/publisher lendo a tabela
      `OutboxMessages` e publicando no RabbitMQ. Hoje o Outbox só grava, nunca publica —
      ou seja, mesmo o serviço "completo" nunca dispara o fluxo de verdade.
- [ ] Implementar `OutboxPublisherService` (BackgroundService) em
      `DispatchService.Infrastructure`: poll periódico nos pendentes
      (`IOutboxRepository.GetPendentesAsync`), deserializa por `EventType`, publica via
      `IPublishEndpoint`, marca processado/registra falha com backoff
- [ ] Remover `Npgsql.EntityFrameworkCore.PostgreSQL` do
      `DispatchService.Infrastructure.csproj` (não usado — projeto já é SQL Server)

### DeliveryService ✅ concluído (2026-09-04)
- [x] Domain: entidade `Entrega` (status: PendenteDeTentativa/Entregue/Falhou, tentativas, timestamps)
- [x] Infrastructure: `DeliveryDbContext` (EF Core + SQL Server) + migration inicial
- [x] Application: `ProcessarEntregaCommandHandler` — idempotente por RecadoId, simula
      tentativa de entrega (~70% sucesso), decide se é falha temporária ou definitiva
- [x] Integration: `RecadoCriadoEventConsumer` publica `EntregaConcluidaEvent`/
      `EntregaFalhouEvent` e `EnviarNotificacaoCommand` conforme o resultado; falha
      temporária relança exceção para o retry/circuit breaker do MassTransit agirem
- [x] Retry (`UseMessageRetry` com os intervalos de `MessagingTopology.RetryPolicy`) +
      Circuit Breaker (`UseCircuitBreaker`) configurados no bus do worker

### InboxService ✅ concluído (2026-09-04)
- [x] Domain: `ItemTimeline` (materialized view, uma linha por RecadoId, chave natural)
- [x] Infrastructure: `InboxDbContext` (EF Core + SQL Server) + migration inicial
- [x] Application: `RegistrarCriacaoNaTimelineCommandHandler` +
      `AtualizarStatusNaTimelineCommandHandler` — idempotentes e tolerantes a
      chegada fora de ordem (evento de entrega antes do de criação, já que são
      exchanges independentes sem garantia de ordem entre si) via registro provisório
- [x] Implementados de fato os 3 consumers: `RecadoCriadoEventConsumer` (estava vazio),
      `EntregaConcluidaEventConsumer` e `EntregaFalhouEventConsumer` (novos)
- [x] `InboxService.Host.Api` nova (minimal API, sem controllers) —
      `GET /api/inbox/{destinatario}` via `ObterTimelineQuery` (lado de leitura do CQRS),
      registrada na `.sln`

### NotificationService ✅ concluído (2026-09-04)
- [x] Domain: porta `ICanalNotificacao` (Strategy) + VO `NotificacaoMensagem`
- [x] Infrastructure: `ConsoleCanalNotificacao` — estratégia concreta simulada (log
      estruturado); trocar por Email/SMS/Push depois é só implementar a interface de
      novo e registrar no DI, Application não muda
- [x] Application: `ProcessarNotificacaoCommandHandler` (Template Method — monta a
      mensagem, delega ao canal, trata sucesso/erro de forma uniforme)
- [x] Integration: `EnviarNotificacaoCommandConsumer` implementado de verdade (antes só
      logava e simulava)

## Fase 3 — Observabilidade fina + testes

- [x] `Tests/GuildaMensageria.AppHost.Tests` (xUnit + `Aspire.Hosting.Testing`) — sobe o
      AppHost inteiro de verdade (containers), cria um recado via API do Dispatch e
      espera a timeline do Inbox refletir um status final (Entregue/Falhou), provando o
      fluxo ponta-a-ponta: Outbox → RabbitMQ → Delivery → RabbitMQ → Inbox. **Compila,
      mas não foi executado** — Docker Desktop indisponível nesta sessão. Rodar
      `dotnet test` com Docker ligado é o próximo passo de validação real.
- [ ] Validar tracing distribuído ponta-a-ponta no Aspire Dashboard (visual, precisa
      rodar o AppHost com Docker ligado e olhar a aba de traces)
- [ ] Revisar DLQs: forçar uma falha proposital e confirmar que a mensagem cai na `.dlq`
      (também precisa de runtime real)

## Fase 4 — Painel de domínio (stretch, o lado "bonitinho")

Diferente do Aspire Dashboard (que é operacional/infra), esse é um painel dos **dados do
domínio**: recados enviados, status de entrega, timeline, notificações — a parte temática
da Guilda.

- [ ] Expor endpoint JSON simples no `InboxService.Host.Api` (lista de recados + status)
- [ ] Painel visual (pode nascer como Artifact do Claude consumindo esse endpoint, ou uma
      página Blazor Server dentro da própria solução se quiser algo self-hosted)

---

## Decisões desta rodada (2026-09-04)

- **Escopo:** Fases 0–3 (backend funcional ponta-a-ponta com Aspire). Fase 4 (painel de
  domínio visual) fica para uma rodada futura.
- **Branch:** direto na `main`, commits incrementais por fase/serviço, push periódico.
- **Execução autônoma:** sem pausas para confirmação. Se o contexto for resumido/cortado,
  a continuação deve ler este arquivo, ver o que está marcado `[x]`, rodar `dotnet build`
  para confirmar o estado real, e seguir do próximo item não marcado.
- Fase 0 reduzida ao essencial (remover órfãos). `Directory.Build.props` /
  `Directory.Packages.props` viraram itens opcionais no fim da lista, não bloqueiam nada.

## O que falta pra fechar as Fases 0–3

Só falta validação em **runtime real** — tudo que dependia de código está feito e
compilando. Com Docker Desktop ligado:

1. `dotnet run --project GuildaMensageria.AppHost` (ou `aspire run`) e conferir no
   Aspire Dashboard que os 5 serviços + RabbitMQ + SQL Server sobem saudáveis
2. Testar o fluxo manualmente: `POST /api/recados` no Dispatch, depois
   `GET /api/inbox/{destinatario}` no Inbox até aparecer `Entregue` ou `Falhou`
3. `dotnet test Tests/GuildaMensageria.AppHost.Tests` — roda o smoke test automatizado
   do item acima
4. Olhar a aba de traces do Dashboard pra confirmar que o trace distribuído atravessa
   os 4 serviços num único fluxo
5. Forçar uma falha (ex.: derrubar o SQL Server do DeliveryService no meio de um
   processamento) e confirmar que a mensagem cai na DLQ depois de esgotar os retries

Se algo desses passos não se comportar como esperado, é o próximo ponto de ajuste —
não precisa replanejar do zero, só voltar num item específico.

## Log de Progresso

- **2026-09-04** — Levantamento do estado atual do repo, criação deste roadmap e do
  dashboard visual. Escopo travado em Fases 0–3, direto na main.
- **2026-09-04** — Fases 0, 1, 2 e 3 implementadas de ponta a ponta (código): faxina de
  órfãos, fix do Outbox do Dispatch, migração de todo o solution para net10.0 (achado:
  máquina sem runtime .NET 9), DeliveryService/InboxService/NotificationService
  implementados, Aspire (AppHost + ServiceDefaults) orquestrando os 5 serviços +
  RabbitMQ + SQL Server, teste de integração ponta-a-ponta escrito. Solution inteira
  (25 projetos) compila com 0 warnings/0 erros. Commits e push feitos a cada etapa.
  Falta só a validação em runtime com Docker Desktop ligado (ver seção acima) — nada
  disso foi possível verificar nesta sessão porque o Docker não estava rodando na
  máquina. Fase 4 (painel de domínio visual) fica pra uma próxima rodada.
