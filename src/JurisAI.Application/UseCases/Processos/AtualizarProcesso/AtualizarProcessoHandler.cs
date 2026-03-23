namespace JurisAI.Application.UseCases.Processos.AtualizarProcesso;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class AtualizarProcessoHandler : BaseHandler<AtualizarProcessoHandler>
{
    private readonly IProcessoRepository _processoRepository;

    public AtualizarProcessoHandler(
        IProcessoRepository processoRepository,
        ILogger<AtualizarProcessoHandler> logger) : base(logger)
    {
        _processoRepository = processoRepository;
    }

    public async Task<Result<ProcessoDto>> HandleAsync(AtualizarProcessoCommand command, CancellationToken ct = default)
    {
        var getResult = await _processoRepository.GetByIdAsync(command.UserId, command.ProcessoId, ct);
        if (!getResult.IsSuccess)
            return Result<ProcessoDto>.Failure(getResult.Error!);

        var processo = getResult.Value!;

        var updateResult = processo.Atualizar(
            command.Titulo, command.Fase, command.Tribunal,
            command.Vara, command.JuizResponsavel,
            command.ParteAdversa, command.Observacoes);

        if (!updateResult.IsSuccess)
            return Result<ProcessoDto>.Failure(updateResult.Error!);

        processo.AtualizarStatus(command.Status);

        var saveResult = await _processoRepository.UpdateAsync(processo, ct);
        if (!saveResult.IsSuccess)
            return Result<ProcessoDto>.Failure(saveResult.Error!);

        var p = saveResult.Value!;
        return Result<ProcessoDto>.Success(new ProcessoDto(
            p.Id, p.NumeroCNJ.Value, p.ClienteId, null,
            p.Titulo, p.TipoAcao, p.Fase, p.Status,
            p.Tribunal, p.Vara, p.JuizResponsavel,
            p.ParteAdversa, p.Observacoes,
            p.UltimaMovimentacao, p.ProximoPrazo,
            p.ProximoPrazo.HasValue ? (int)(p.ProximoPrazo.Value - DateTime.UtcNow).TotalDays : null,
            p.CreatedAt, p.UpdatedAt));
    }
}
