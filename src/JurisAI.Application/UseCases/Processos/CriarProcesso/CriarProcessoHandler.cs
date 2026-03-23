namespace JurisAI.Application.UseCases.Processos.CriarProcesso;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class CriarProcessoHandler : BaseHandler<CriarProcessoHandler>
{
    private readonly IProcessoRepository _processoRepository;
    private readonly IClienteRepository _clienteRepository;

    public CriarProcessoHandler(
        IProcessoRepository processoRepository,
        IClienteRepository clienteRepository,
        ILogger<CriarProcessoHandler> logger) : base(logger)
    {
        _processoRepository = processoRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<ProcessoDto>> HandleAsync(CriarProcessoCommand command, CancellationToken ct = default)
    {
        // Verifica se o cliente existe
        var clienteResult = await _clienteRepository.GetByIdAsync(command.UserId, command.ClienteId, ct);
        if (!clienteResult.IsSuccess)
            return Result<ProcessoDto>.Failure(clienteResult.Error!);

        // Cria a entidade — a validação de domínio ocorre aqui
        var processoResult = Processo.Criar(
            command.UserId,
            command.NumeroCNJ,
            command.ClienteId,
            command.Titulo,
            command.TipoAcao,
            command.Fase,
            command.Tribunal,
            command.Vara,
            command.JuizResponsavel,
            command.ParteAdversa,
            command.Observacoes);

        if (!processoResult.IsSuccess)
            return Result<ProcessoDto>.Failure(processoResult.Error!);

        var saveResult = await _processoRepository.CreateAsync(processoResult.Value!, ct);
        if (!saveResult.IsSuccess)
            return Result<ProcessoDto>.Failure(saveResult.Error!);

        var processo = saveResult.Value!;
        var cliente = clienteResult.Value!;

        Logger.LogInformation("Processo {ProcessoId} criado para o usuário {UserId}", processo.Id, command.UserId);

        return Result<ProcessoDto>.Success(new ProcessoDto(
            processo.Id,
            processo.NumeroCNJ.Value,
            processo.ClienteId,
            cliente.Nome,
            processo.Titulo,
            processo.TipoAcao,
            processo.Fase,
            processo.Status,
            processo.Tribunal,
            processo.Vara,
            processo.JuizResponsavel,
            processo.ParteAdversa,
            processo.Observacoes,
            processo.UltimaMovimentacao,
            processo.ProximoPrazo,
            processo.ProximoPrazo.HasValue ? (int)(processo.ProximoPrazo.Value - DateTime.UtcNow).TotalDays : null,
            processo.CreatedAt,
            processo.UpdatedAt));
    }
}
