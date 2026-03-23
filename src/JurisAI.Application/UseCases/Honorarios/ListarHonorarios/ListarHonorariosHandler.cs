namespace JurisAI.Application.UseCases.Honorarios.ListarHonorarios;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class ListarHonorariosHandler : BaseHandler<ListarHonorariosHandler>
{
    private readonly IHonorarioRepository _honorarioRepository;

    public ListarHonorariosHandler(
        IHonorarioRepository honorarioRepository,
        ILogger<ListarHonorariosHandler> logger) : base(logger)
    {
        _honorarioRepository = honorarioRepository;
    }

    public async Task<Result<IReadOnlyList<HonorarioDto>>> HandleAsync(ListarHonorariosQuery query, CancellationToken ct = default)
    {
        var result = await _honorarioRepository.GetByUserIdAsync(query.UserId, query.ApenasPendentes, ct);

        return result.Match(
            honorarios => Result<IReadOnlyList<HonorarioDto>>.Success(
                honorarios.Select(h => new HonorarioDto(
                    h.Id, h.ClienteId, null, h.ProcessoId, null,
                    h.Descricao, h.Valor, h.DataVencimento, h.DataPagamento,
                    h.Pago, h.FormaPagamento, h.Observacoes, h.CreatedAt)).ToList().AsReadOnly()),
            error => Result<IReadOnlyList<HonorarioDto>>.Failure(error));
    }
}
