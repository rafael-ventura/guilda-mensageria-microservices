namespace DispatchService.Domain.Entities;

/// <summary>
/// Entidade principal do DispatchService - representa um recado a ser entregue
/// </summary>
public class Recado
{
    public Guid Id { get; private set; }
    public string Remetente { get; private set; } = string.Empty;
    public string Destinatario { get; private set; } = string.Empty;
    public string Conteudo { get; private set; } = string.Empty;
    public string? EnderecoEntrega { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public StatusRecado Status { get; private set; }

    // EF Core constructor
    private Recado() { }

    public Recado(string remetente, string destinatario, string conteudo, string? enderecoEntrega = null)
    {
        if (string.IsNullOrWhiteSpace(remetente))
            throw new ArgumentException("Remetente é obrigatório", nameof(remetente));
        
        if (string.IsNullOrWhiteSpace(destinatario))
            throw new ArgumentException("Destinatário é obrigatório", nameof(destinatario));
        
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new ArgumentException("Conteúdo é obrigatório", nameof(conteudo));

        Id = Guid.NewGuid();
        Remetente = remetente.Trim();
        Destinatario = destinatario.Trim();
        Conteudo = conteudo.Trim();
        EnderecoEntrega = enderecoEntrega?.Trim();
        CriadoEm = DateTime.UtcNow;
        Status = StatusRecado.Criado;
    }

    public void AtualizarStatus(StatusRecado novoStatus)
    {
        Status = novoStatus;
        AtualizadoEm = DateTime.UtcNow;
    }
}

/// <summary>
/// Status possíveis de um recado
/// </summary>
public enum StatusRecado
{
    Criado = 1,
    EmProcessamento = 2,
    Entregue = 3,
    Falhou = 4
}
