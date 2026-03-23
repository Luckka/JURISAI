namespace JurisAI.Application.UseCases.Processos.ConsultarPrazosCNJ;

using JurisAI.Application.Common;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using JurisAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

public class ConsultarPrazosCNJHandler : BaseHandler<ConsultarPrazosCNJHandler>
{
    private readonly IProcessoRepository _processoRepository;
    private readonly ICNJService _cnjService;

    public ConsultarPrazosCNJHandler(
        IProcessoRepository processoRepository,
        ICNJService cnjService,
        ILogger<ConsultarPrazosCNJHandler> logger) : base(logger)
    {
        _processoRepository = processoRepository;
        _cnjService = cnjService;
    }

    public async Task<Result<PrazosCNJResponse>> HandleAsync(ConsultarPrazosCNJCommand command, CancellationToken ct = default)
    {
        var processoResult = await _processoRepository.GetByIdAsync(command.UserId, command.ProcessoId, ct);
        if (!processoResult.IsSuccess)
            return Result<PrazosCNJResponse>.Failure(processoResult.Error!);

        var processo = processoResult.Value!;
        var cnjResult = await _cnjService.ConsultarPrazosAsync(processo.NumeroCNJ.Value, ct);

        if (!cnjResult.IsSuccess)
        {
            Logger.LogWarning("Falha ao consultar CNJ para processo {ProcessoId}: {Error}",
                command.ProcessoId, cnjResult.Error?.Message);
            return Result<PrazosCNJResponse>.Failure(cnjResult.Error!);
        }

        // Atualiza o próximo prazo no processo se disponível
        if (cnjResult.Value!.ProximoPrazo.HasValue)
        {
            processo.DefinirProximoPrazo(cnjResult.Value.ProximoPrazo.Value);
            await _processoRepository.UpdateAsync(processo, ct);
        }

        return cnjResult;
    }
}
