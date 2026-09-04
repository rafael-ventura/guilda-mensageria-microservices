using InboxService.Application.Queries;
using InboxService.Domain.Repositories;
using MediatR;

namespace InboxService.Application.Handlers;

/// <summary>
/// Handler da query de leitura da timeline (lado de query do CQRS)
/// </summary>
public class ObterTimelineQueryHandler : IRequestHandler<ObterTimelineQuery, IEnumerable<ItemTimelineDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObterTimelineQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ItemTimelineDto>> Handle(ObterTimelineQuery request, CancellationToken cancellationToken)
    {
        var itens = await _unitOfWork.ItensTimeline.GetByDestinatarioAsync(request.Destinatario, cancellationToken);

        return itens.Select(i => new ItemTimelineDto(
            i.RecadoId,
            i.Remetente,
            i.Destinatario,
            i.Conteudo,
            i.Status.ToString(),
            i.CriadoEm,
            i.EntregueEm,
            i.MotivoFalha));
    }
}
