using InboxService.Application.Commands;
using InboxService.Domain.Entities;
using InboxService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InboxService.Application.Handlers;

/// <summary>
/// Materializa os eventos de entrega (concluída/falhou) como atualização de status na
/// timeline. Se a linha ainda não existir — o RecadoCriadoEvent pode chegar depois,
/// já que são exchanges independentes sem garantia de ordem — cria um registro
/// provisório para não perder a informação; RegistrarCriacaoNaTimelineCommandHandler
/// completa os dados quando (se) o evento de criação chegar.
/// </summary>
public class AtualizarStatusNaTimelineCommandHandler : IRequestHandler<AtualizarStatusNaTimelineCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AtualizarStatusNaTimelineCommandHandler> _logger;

    public AtualizarStatusNaTimelineCommandHandler(IUnitOfWork unitOfWork, ILogger<AtualizarStatusNaTimelineCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AtualizarStatusNaTimelineCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.ItensTimeline.GetByRecadoIdAsync(request.RecadoId, cancellationToken);

        if (item is null)
        {
            item = ItemTimeline.CriarProvisorio(request.RecadoId, request.Destinatario);
            await _unitOfWork.ItensTimeline.AddAsync(item, cancellationToken);
        }

        switch (request.NovoStatus)
        {
            case StatusTimelineRecado.Entregue:
                item.MarcarComoEntregue(request.OcorreuEm);
                break;
            case StatusTimelineRecado.Falhou:
                item.MarcarComoFalhou(request.MotivoFalha ?? "Falha não especificada");
                break;
            default:
                _logger.LogWarning("Status {Status} inesperado para atualização de timeline - RecadoId: {RecadoId}", request.NovoStatus, request.RecadoId);
                return;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "🔄 Timeline atualizada - RecadoId: {RecadoId}, Status: {Status}",
            request.RecadoId, request.NovoStatus);
    }
}
