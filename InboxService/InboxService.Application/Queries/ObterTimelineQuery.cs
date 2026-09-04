using MediatR;

namespace InboxService.Application.Queries;

/// <summary>
/// Query para obter a timeline de um destinatário
/// </summary>
public record ObterTimelineQuery(string Destinatario) : IRequest<IEnumerable<ItemTimelineDto>>;

/// <summary>
/// Projeção de leitura de um item da timeline
/// </summary>
public record ItemTimelineDto(
    Guid RecadoId,
    string Remetente,
    string Destinatario,
    string Conteudo,
    string Status,
    DateTime CriadoEm,
    DateTime? EntregueEm,
    string? MotivoFalha
);
