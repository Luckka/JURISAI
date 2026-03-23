namespace JurisAI.Application.UseCases.Clientes.CriarCliente;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class CriarClienteHandler : BaseHandler<CriarClienteHandler>
{
    private readonly IClienteRepository _clienteRepository;

    public CriarClienteHandler(
        IClienteRepository clienteRepository,
        ILogger<CriarClienteHandler> logger) : base(logger)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<ClienteDto>> HandleAsync(CriarClienteCommand command, CancellationToken ct = default)
    {
        var clienteResult = Cliente.Criar(
            command.UserId, command.Nome, command.Documento,
            command.Email, command.Telefone, command.Endereco, command.Observacoes);

        if (!clienteResult.IsSuccess)
            return Result<ClienteDto>.Failure(clienteResult.Error!);

        var saveResult = await _clienteRepository.CreateAsync(clienteResult.Value!, ct);
        if (!saveResult.IsSuccess)
            return Result<ClienteDto>.Failure(saveResult.Error!);

        var c = saveResult.Value!;
        Logger.LogInformation("Cliente {ClienteId} criado para o usuário {UserId}", c.Id, command.UserId);

        return Result<ClienteDto>.Success(new ClienteDto(
            c.Id, c.Nome, c.Documento.Value, c.Documento.Formatted,
            c.Email.Value, c.Telefone, c.Endereco, c.Observacoes,
            c.Ativo, 0, c.CreatedAt, c.UpdatedAt));
    }
}
