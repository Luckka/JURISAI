namespace JurisAI.Application.UseCases.Clientes.ObterCliente;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class ObterClienteHandler : BaseHandler<ObterClienteHandler>
{
    private readonly IClienteRepository _clienteRepository;

    public ObterClienteHandler(
        IClienteRepository clienteRepository,
        ILogger<ObterClienteHandler> logger) : base(logger)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<ClienteDto>> HandleAsync(ObterClienteQuery query, CancellationToken ct = default)
    {
        var result = await _clienteRepository.GetByIdAsync(query.UserId, query.ClienteId);
        return result.Match(
            c => Result<ClienteDto>.Success(new ClienteDto(
                c.Id, c.Nome, c.Documento.Value, c.Documento.Formatted,
                c.Email.Value, c.Telefone, c.Endereco, c.Observacoes,
                c.Ativo, 0, c.CreatedAt, c.UpdatedAt)),
            error => Result<ClienteDto>.Failure(error));
    }
}
