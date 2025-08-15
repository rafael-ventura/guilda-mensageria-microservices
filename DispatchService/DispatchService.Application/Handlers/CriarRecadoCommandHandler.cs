using DispatchService.Application.Commands;
using DispatchService.Domain.Entities;
using DispatchService.Domain.Repositories;
using GuildaMensageria.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DispatchService.Application.Handlers;

/// <summary>
/// Handler para o comando de criação de recado
/// </summary>
public class CriarRecadoCommandHandler : IRequestHandler<CriarRecadoCommand, CriarRecadoResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CriarRecadoCommandHandler> _logger;

    public CriarRecadoCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CriarRecadoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CriarRecadoResult> Handle(CriarRecadoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando criação de recado - Remetente: {Remetente}, Destinatario: {Destinatario}",
                request.Remetente, request.Destinatario);

            // Criar entidade de domínio
            var recado = new Recado(
                request.Remetente,
                request.Destinatario,
                request.Conteudo,
                request.EnderecoEntrega);

            // Criar evento para outbox
            var evento = new RecadoCriadoEvent
            {
                RecadoId = recado.Id,
                Remetente = recado.Remetente,
                Destinatario = recado.Destinatario,
                Conteudo = recado.Conteudo,
                CriadoEm = recado.CriadoEm,
                EnderecoEntrega = recado.EnderecoEntrega
            };

            var outboxMessage = new OutboxMessage(nameof(RecadoCriadoEvent), evento);

            // Iniciar transação
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Persistir recado e evento na mesma transação
                await _unitOfWork.Recados.AddAsync(recado, cancellationToken);
                await _unitOfWork.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Recado criado com sucesso - RecadoId: {RecadoId}",
                    recado.Id);

                return new CriarRecadoResult(recado.Id, true);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para criação de recado");
            return new CriarRecadoResult(Guid.Empty, false, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar recado");
            return new CriarRecadoResult(Guid.Empty, false, "Erro interno do servidor");
        }
    }
}
