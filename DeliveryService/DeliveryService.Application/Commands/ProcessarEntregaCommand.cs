using MediatR;

namespace DeliveryService.Application.Commands;

/// <summary>
/// Command para processar (tentar) a entrega de um recado
/// </summary>
public record ProcessarEntregaCommand(
    Guid RecadoId,
    string Destinatario,
    string? EnderecoEntrega
) : IRequest<ProcessarEntregaResult>;

/// <summary>
/// Resultado de uma tentativa de processamento de entrega
/// </summary>
public record ProcessarEntregaResult(
    bool Entregue,
    bool TentativaFinal,
    int Tentativas,
    string? Motivo
);
