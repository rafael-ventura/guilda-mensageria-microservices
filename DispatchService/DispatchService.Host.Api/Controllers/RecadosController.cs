using DispatchService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DispatchService.Host.Api.Controllers;

/// <summary>
/// Controller para gerenciar recados
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RecadosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecadosController> _logger;

    public RecadosController(IMediator mediator, ILogger<RecadosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo recado
    /// </summary>
    /// <param name="request">Dados do recado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da criação</returns>
    [HttpPost]
    public async Task<ActionResult<CriarRecadoResponse>> CriarRecado(
        [FromBody] CriarRecadoRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recebida requisição para criar recado");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CriarRecadoCommand(
            request.Remetente,
            request.Destinatario,
            request.Conteudo,
            request.EnderecoEntrega);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Sucesso)
        {
            return BadRequest(new { erro = result.MensagemErro });
        }

        var response = new CriarRecadoResponse(
            result.RecadoId,
            "Recado criado com sucesso e será processado em breve");

        return CreatedAtAction(
            nameof(ObterRecado),
            new { id = result.RecadoId },
            response);
    }

    /// <summary>
    /// Obtém um recado por ID
    /// </summary>
    /// <param name="id">ID do recado</param>
    /// <returns>Dados do recado</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> ObterRecado(Guid id)
    {
        // TODO: Implementar query para obter recado
        return Ok(new { id, status = "Em desenvolvimento" });
    }
}

/// <summary>
/// Request para criação de recado
/// </summary>
public record CriarRecadoRequest(
    string Remetente,
    string Destinatario,
    string Conteudo,
    string? EnderecoEntrega = null
);

/// <summary>
/// Response da criação de recado
/// </summary>
public record CriarRecadoResponse(
    Guid RecadoId,
    string Mensagem
);
