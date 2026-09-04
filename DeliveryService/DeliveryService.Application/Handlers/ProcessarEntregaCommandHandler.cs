using DeliveryService.Application.Commands;
using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DeliveryService.Application.Handlers;

/// <summary>
/// Handler que simula a tentativa de entrega de um recado. Idempotente por RecadoId:
/// um reprocessamento (redelivery da mensagem) sobre uma entrega já resolvida não
/// tenta de novo, só devolve o resultado anterior.
/// </summary>
public class ProcessarEntregaCommandHandler : IRequestHandler<ProcessarEntregaCommand, ProcessarEntregaResult>
{
    private const int MaxTentativas = 3;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessarEntregaCommandHandler> _logger;

    public ProcessarEntregaCommandHandler(IUnitOfWork unitOfWork, ILogger<ProcessarEntregaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProcessarEntregaResult> Handle(ProcessarEntregaCommand request, CancellationToken cancellationToken)
    {
        var entrega = await _unitOfWork.Entregas.GetByRecadoIdAsync(request.RecadoId, cancellationToken);

        if (entrega is null)
        {
            entrega = new Entrega(request.RecadoId, request.Destinatario, request.EnderecoEntrega);
            await _unitOfWork.Entregas.AddAsync(entrega, cancellationToken);
        }

        if (entrega.Status != StatusEntrega.PendenteDeTentativa)
        {
            _logger.LogInformation(
                "Entrega {RecadoId} já resolvida (Status: {Status}), ignorando reprocessamento",
                request.RecadoId, entrega.Status);

            return new ProcessarEntregaResult(entrega.Status == StatusEntrega.Entregue, true, entrega.Tentativas, entrega.UltimoErro);
        }

        entrega.RegistrarTentativa();

        // Simula a tentativa de entrega (transportadora, rede, etc.)
        await Task.Delay(300, cancellationToken);
        var sucesso = Random.Shared.NextDouble() < 0.7;

        if (sucesso)
        {
            entrega.MarcarComoEntregue();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "✅ Entrega concluída - RecadoId: {RecadoId}, Tentativas: {Tentativas}",
                request.RecadoId, entrega.Tentativas);

            return new ProcessarEntregaResult(true, true, entrega.Tentativas, null);
        }

        const string motivo = "Falha simulada na tentativa de entrega";
        var tentativaFinal = entrega.Tentativas >= MaxTentativas;

        if (tentativaFinal)
            entrega.MarcarComoFalhouDefinitivamente(motivo);
        else
            entrega.RegistrarFalhaTemporaria(motivo);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "❌ Tentativa de entrega falhou - RecadoId: {RecadoId}, Tentativa: {Tentativa}/{Max}, Final: {Final}",
            request.RecadoId, entrega.Tentativas, MaxTentativas, tentativaFinal);

        return new ProcessarEntregaResult(false, tentativaFinal, entrega.Tentativas, motivo);
    }
}
