namespace JurisAI.Application.UseCases.Processos.ListarProcessos;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class ListarProcessosHandler : BaseHandler<ListarProcessosHandler>
{
    private readonly IProcessoRepository _processoRepository;

    public ListarProcessosHandler(
        IProcessoRepository processoRepository,
        ILogger<ListarProcessosHandler> logger) : base(logger)
    {
        _processoRepository = processoRepository;
    }

    public async Task<Result<IReadOnlyList<ProcessoDto>>> HandleAsync(ListarProcessosQuery query, CancellationToken ct = default)
    {
        var result = await _processoRepository.GetByUserIdAsync(query.UserId, query.Page, query.PageSize, ct);

        return result.Match(
            processos => Result<IReadOnlyList<ProcessoDto>>.Success(
                processos.Select(p => new ProcessoDto(
                    p.Id, p.NumeroCNJ.Value, p.ClienteId, null,
                    p.Titulo, p.TipoAcao, p.Fase, p.Status,
                    p.Tribunal, p.Vara, p.JuizResponsavel,
                    p.ParteAdversa, p.Observacoes,
                    p.UltimaMovimentacao, p.ProximoPrazo,
                    p.ProximoPrazo.HasValue ? (int)(p.ProximoPrazo.Value - DateTime.UtcNow).TotalDays : null,
                    p.CreatedAt, p.UpdatedAt)).ToList().AsReadOnly()),
            error => Result<IReadOnlyList<ProcessoDto>>.Failure(error));
    }
}
