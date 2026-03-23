namespace JurisAI.Domain.Interfaces.Services;

using JurisAI.Domain.Common;

public record MovimentacaoCNJ(string Data, string Descricao, string Tipo);
public record PrazosCNJResponse(string NumeroCNJ, IReadOnlyList<MovimentacaoCNJ> Movimentacoes, DateTime? ProximoPrazo);

public interface ICNJService
{
    Task<Result<PrazosCNJResponse>> ConsultarPrazosAsync(string numeroCNJ, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovimentacaoCNJ>>> GetMovimentacoesAsync(string numeroCNJ, CancellationToken ct = default);
}
