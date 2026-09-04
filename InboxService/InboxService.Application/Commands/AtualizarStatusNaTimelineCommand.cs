using InboxService.Domain.Entities;
using MediatR;

namespace InboxService.Application.Commands;

/// <summary>
/// Command para atualizar o status de entrega de um recado na timeline do destinatário
/// </summary>
public record AtualizarStatusNaTimelineCommand(
    Guid RecadoId,
    string Destinatario,
    StatusTimelineRecado NovoStatus,
    DateTime OcorreuEm,
    string? MotivoFalha
) : IRequest;
