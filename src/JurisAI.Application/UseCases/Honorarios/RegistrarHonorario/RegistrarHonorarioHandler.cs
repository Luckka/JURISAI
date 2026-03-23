namespace JurisAI.Application.UseCases.Honorarios.RegistrarHonorario;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class RegistrarHonorarioHandler : BaseHandler<RegistrarHonorarioHandler>
{
    private readonly IHonorarioRepository _honorarioRepository;
    private readonly IClienteRepository _clienteRepository;

    public RegistrarHonorarioHandler(
        IHonorarioRepository honorarioRepository,
        IClienteRepository clienteRepository,
        ILogger<RegistrarHonorarioHandler> logger) : base(logger)
    {
        _honorarioRepository = honorarioRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<HonorarioDto>> HandleAsync(RegistrarHonorarioCommand command, CancellationToken ct = default)
    {
        var clienteResult = await _clienteRepository.GetByIdAsync(command.UserId, command.ClienteId, ct);
        if (!clienteResult.IsSuccess)
            return Result<HonorarioDto>.Failure(clienteResult.Error!);

        var honorarioResult = Honorario.Registrar(
            command.UserId, command.ClienteId, command.Descricao,
            command.Valor, command.DataVencimento, command.ProcessoId, command.Observacoes);

        if (!honorarioResult.IsSuccess)
            return Result<HonorarioDto>.Failure(honorarioResult.Error!);

        var saveResult = await _honorarioRepository.CreateAsync(honorarioResult.Value!, ct);
        if (!saveResult.IsSuccess)
            return Result<HonorarioDto>.Failure(saveResult.Error!);

        var h = saveResult.Value!;
        var cliente = clienteResult.Value!;

        return Result<HonorarioDto>.Success(new HonorarioDto(
            h.Id, h.ClienteId, cliente.Nome, h.ProcessoId, null,
            h.Descricao, h.Valor, h.DataVencimento, h.DataPagamento,
            h.Pago, h.FormaPagamento, h.Observacoes, h.CreatedAt));
    }
}
