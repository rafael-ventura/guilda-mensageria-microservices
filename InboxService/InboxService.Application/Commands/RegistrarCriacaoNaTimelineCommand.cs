using MediatR;

namespace InboxService.Application.Commands;

/// <summary>
/// Command para registrar a criação de um recado na timeline do destinatário
/// </summary>
public record RegistrarCriacaoNaTimelineCommand(
    Guid RecadoId,
    string Remetente,
    string Destinatario,
    string Conteudo,
    string? EnderecoEntrega,
    DateTime CriadoEm
) : IRequest;
