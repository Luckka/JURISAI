namespace JurisAI.Application.UseCases.Clientes.ListarClientes;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class ListarClientesHandler : BaseHandler<ListarClientesHandler>
{
    private readonly IClienteRepository _clienteRepository;

    public ListarClientesHandler(
        IClienteRepository clienteRepository,
        ILogger<ListarClientesHandler> logger) : base(logger)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<IReadOnlyList<ClienteDto>>> HandleAsync(ListarClientesQuery query, CancellationToken ct = default)
    {
        var result = await _clienteRepository.GetByUserIdAsync(query.UserId, query.Page, query.PageSize, ct);

        return result.Match(
            clientes => Result<IReadOnlyList<ClienteDto>>.Success(
                clientes.Select(c => new ClienteDto(
                    c.Id, c.Nome, c.Documento.Value, c.Documento.Formatted,
                    c.Email.Value, c.Telefone, c.Endereco, c.Observacoes,
                    c.Ativo, 0, c.CreatedAt, c.UpdatedAt)).ToList().AsReadOnly()),
            error => Result<IReadOnlyList<ClienteDto>>.Failure(error));
    }
}
