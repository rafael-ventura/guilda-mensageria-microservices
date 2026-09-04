using InboxService.Application.Commands;
using InboxService.Domain.Entities;
using InboxService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InboxService.Application.Handlers;

/// <summary>
/// Materializa o RecadoCriadoEvent como uma linha na timeline do destinatário.
/// Idempotente: se a linha já existe (por exemplo, criada provisoriamente por um
/// evento de entrega que chegou antes), só preenche os dados que faltavam.
/// </summary>
public class RegistrarCriacaoNaTimelineCommandHandler : IRequestHandler<RegistrarCriacaoNaTimelineCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegistrarCriacaoNaTimelineCommandHandler> _logger;

    public RegistrarCriacaoNaTimelineCommandHandler(IUnitOfWork unitOfWork, ILogger<RegistrarCriacaoNaTimelineCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RegistrarCriacaoNaTimelineCommand request, CancellationToken cancellationToken)
    {
        var existente = await _unitOfWork.ItensTimeline.GetByRecadoIdAsync(request.RecadoId, cancellationToken);

        if (existente is not null)
        {
            _logger.LogInformation(
                "Item de timeline {RecadoId} já existia (provisório), ignorando reprocessamento de criação",
                request.RecadoId);
            return;
        }

        var item = new ItemTimeline(
            request.RecadoId,
            request.Remetente,
            request.Destinatario,
            request.Conteudo,
            request.EnderecoEntrega,
            request.CriadoEm);

        await _unitOfWork.ItensTimeline.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "📥 Recado registrado na timeline - RecadoId: {RecadoId}, Destinatario: {Destinatario}",
            request.RecadoId, request.Destinatario);
    }
}
