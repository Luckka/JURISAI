namespace JurisAI.Application.UseCases.Processos.ObterProcesso;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class ObterProcessoHandler : BaseHandler<ObterProcessoHandler>
{
    private readonly IProcessoRepository _processoRepository;
    private readonly IClienteRepository _clienteRepository;

    public ObterProcessoHandler(
        IProcessoRepository processoRepository,
        IClienteRepository clienteRepository,
        ILogger<ObterProcessoHandler> logger) : base(logger)
    {
        _processoRepository = processoRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<ProcessoDto>> HandleAsync(ObterProcessoQuery query, CancellationToken ct = default)
    {
        var result = await _processoRepository.GetByIdAsync(query.UserId, query.ProcessoId, ct);
        if (!result.IsSuccess)
            return Result<ProcessoDto>.Failure(result.Error!);

        var processo = result.Value!;
        var clienteResult = await _clienteRepository.GetByIdAsync(query.UserId, processo.ClienteId, ct);
        var clienteNome = clienteResult.IsSuccess ? clienteResult.Value!.Nome : null;

        return Result<ProcessoDto>.Success(new ProcessoDto(
            processo.Id, processo.NumeroCNJ.Value, processo.ClienteId, clienteNome,
            processo.Titulo, processo.TipoAcao, processo.Fase, processo.Status,
            processo.Tribunal, processo.Vara, processo.JuizResponsavel,
            processo.ParteAdversa, processo.Observacoes,
            processo.UltimaMovimentacao, processo.ProximoPrazo,
            processo.ProximoPrazo.HasValue ? (int)(processo.ProximoPrazo.Value - DateTime.UtcNow).TotalDays : null,
            processo.CreatedAt, processo.UpdatedAt));
    }
}
