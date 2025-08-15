using MediatR;

namespace DispatchService.Application.Commands;

/// <summary>
/// Command para criar um novo recado
/// </summary>
public record CriarRecadoCommand(
    string Remetente,
    string Destinatario,
    string Conteudo,
    string? EnderecoEntrega = null
) : IRequest<CriarRecadoResult>;

/// <summary>
/// Resultado do comando de criação de recado
/// </summary>
public record CriarRecadoResult(
    Guid RecadoId,
    bool Sucesso,
    string? MensagemErro = null
);
