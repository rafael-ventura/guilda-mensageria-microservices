namespace DeliveryService.Domain.Entities;

/// <summary>
/// Representa o processo de entrega de um recado — uma linha por RecadoId, idempotente
/// entre reprocessamentos (redelivery do MassTransit).
/// </summary>
public class Entrega
{
    public Guid Id { get; private set; }
    public Guid RecadoId { get; private set; }
    public string Destinatario { get; private set; } = string.Empty;
    public string? EnderecoEntrega { get; private set; }
    public StatusEntrega Status { get; private set; }
    public int Tentativas { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public DateTime? EntregueEm { get; private set; }
    public string? UltimoErro { get; private set; }

    // EF Core constructor
    private Entrega() { }

    public Entrega(Guid recadoId, string destinatario, string? enderecoEntrega)
    {
        if (recadoId == Guid.Empty)
            throw new ArgumentException("RecadoId é obrigatório", nameof(recadoId));

        if (string.IsNullOrWhiteSpace(destinatario))
            throw new ArgumentException("Destinatário é obrigatório", nameof(destinatario));

        Id = Guid.NewGuid();
        RecadoId = recadoId;
        Destinatario = destinatario.Trim();
        EnderecoEntrega = enderecoEntrega?.Trim();
        Status = StatusEntrega.PendenteDeTentativa;
        Tentativas = 0;
        CriadoEm = DateTime.UtcNow;
    }

    public void RegistrarTentativa()
    {
        Tentativas++;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarcarComoEntregue()
    {
        Status = StatusEntrega.Entregue;
        EntregueEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
        UltimoErro = null;
    }

    public void RegistrarFalhaTemporaria(string motivo)
    {
        UltimoErro = motivo;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarcarComoFalhouDefinitivamente(string motivo)
    {
        Status = StatusEntrega.Falhou;
        UltimoErro = motivo;
        AtualizadoEm = DateTime.UtcNow;
    }
}

/// <summary>
/// Status possíveis de uma entrega
/// </summary>
public enum StatusEntrega
{
    PendenteDeTentativa = 1,
    Entregue = 2,
    Falhou = 3
}
