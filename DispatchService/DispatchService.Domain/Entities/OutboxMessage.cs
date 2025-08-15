using System.Text.Json;

namespace DispatchService.Domain.Entities;

/// <summary>
/// Entidade para implementar o Outbox Pattern - garante consistência entre persistência e publicação de eventos
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string EventData { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime? ProcessadoEm { get; private set; }
    public bool Processado { get; private set; }
    public int TentativasProcessamento { get; private set; }
    public DateTime? ProximaTentativaEm { get; private set; }
    public string? ErroUltimoProcessamento { get; private set; }

    // EF Core constructor
    private OutboxMessage() { }

    public OutboxMessage(string eventType, object eventData)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Tipo do evento é obrigatório", nameof(eventType));
        
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));

        Id = Guid.NewGuid();
        EventType = eventType;
        EventData = JsonSerializer.Serialize(eventData);
        CriadoEm = DateTime.UtcNow;
        Processado = false;
        TentativasProcessamento = 0;
    }

    public void MarcarComoProcessado()
    {
        Processado = true;
        ProcessadoEm = DateTime.UtcNow;
        ErroUltimoProcessamento = null;
    }

    public void RegistrarTentativaFalhou(string erro, TimeSpan proximaTentativaEm)
    {
        TentativasProcessamento++;
        ErroUltimoProcessamento = erro;
        ProximaTentativaEm = DateTime.UtcNow.Add(proximaTentativaEm);
    }

    public T? DeserializarEventData<T>() where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(EventData);
        }
        catch
        {
            return null;
        }
    }

    public bool PodeProcessar()
    {
        return !Processado && 
               (ProximaTentativaEm == null || ProximaTentativaEm <= DateTime.UtcNow) &&
               TentativasProcessamento < 5; // Máximo 5 tentativas
    }
}
