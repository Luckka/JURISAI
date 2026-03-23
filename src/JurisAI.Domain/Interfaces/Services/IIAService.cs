namespace JurisAI.Domain.Interfaces.Services;

using JurisAI.Domain.Common;

public record GerarPecaRequest(string TipoPeca, string Contexto, string? ProcessoNumero = null, string? ParteAdversa = null);
public record GerarPecaResponse(string Conteudo, string ModeloUtilizado, int TokensUsados);

public interface IIAService
{
    Task<Result<GerarPecaResponse>> GerarPecaAsync(GerarPecaRequest request, CancellationToken ct = default);
    IAsyncEnumerable<string> GerarPecaStreamAsync(GerarPecaRequest request, CancellationToken ct = default);
}
