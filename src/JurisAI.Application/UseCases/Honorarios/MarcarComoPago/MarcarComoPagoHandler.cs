namespace JurisAI.Application.UseCases.Honorarios.MarcarComoPago;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class MarcarComoPagoHandler : BaseHandler<MarcarComoPagoHandler>
{
    private readonly IHonorarioRepository _honorarioRepository;

    public MarcarComoPagoHandler(
        IHonorarioRepository honorarioRepository,
        ILogger<MarcarComoPagoHandler> logger) : base(logger)
    {
        _honorarioRepository = honorarioRepository;
    }

    public async Task<Result<HonorarioDto>> HandleAsync(MarcarComoPagoCommand command, CancellationToken ct = default)
    {
        var getResult = await _honorarioRepository.GetByIdAsync(command.UserId, command.HonorarioId, ct);
        if (!getResult.IsSuccess)
            return Result<HonorarioDto>.Failure(getResult.Error!);

        var honorario = getResult.Value!;
        var pagarResult = honorario.MarcarComoPago(command.FormaPagamento);
        if (!pagarResult.IsSuccess)
            return Result<HonorarioDto>.Failure(pagarResult.Error!);

        var saveResult = await _honorarioRepository.UpdateAsync(honorario, ct);
        if (!saveResult.IsSuccess)
            return Result<HonorarioDto>.Failure(saveResult.Error!);

        var h = saveResult.Value!;
        return Result<HonorarioDto>.Success(new HonorarioDto(
            h.Id, h.ClienteId, null, h.ProcessoId, null,
            h.Descricao, h.Valor, h.DataVencimento, h.DataPagamento,
            h.Pago, h.FormaPagamento, h.Observacoes, h.CreatedAt));
    }
}
