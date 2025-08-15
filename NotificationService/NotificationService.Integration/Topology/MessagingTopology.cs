namespace NotificationService.Integration.Topology;

/// <summary>
/// Definições de topologia de mensageria para o sistema Guilda dos Mensageiros
/// </summary>
public static class MessagingTopology
{
    // Exchanges (Fanout) - Eventos de integração
    public static class Exchanges
    {
        public const string RecadoEvents = "recado.events";
        public const string EntregaEvents = "entrega.events";
    }

    // Queues (Direct) - Comandos de integração
    public static class Queues
    {
        public const string NotificacaoCommands = "notificacao.commands";
        public const string InboxCommands = "inbox.commands";
    }

    // Dead Letter Queues - Sufixo para filas com falha
    public static class DeadLetterQueues
    {
        public const string Suffix = ".dlq";
        
        public static string GetDlqName(string queueName) => $"{queueName}{Suffix}";
    }

    // Configurações de retry
    public static class RetryPolicy
    {
        public static readonly int[] RetryIntervals = { 1, 5, 15, 30, 60 }; // segundos
        public const int MaxRetryAttempts = 5;
        public const int PrefetchCount = 10;
    }
}
