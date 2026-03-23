namespace JurisAI.Infrastructure.Services;

using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Serviço de consulta ao DataJud - API pública do CNJ.
/// Documentação: https://datajud-wiki.cnj.jus.br/api-publica/
/// </summary>
public class CNJService : ICNJService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CNJService> _logger;
    private const string BaseUrl = "https://api-publica.datajud.cnj.jus.br";

    public CNJService(HttpClient httpClient, ILogger<CNJService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", "ApiKey cDZHYzlZa0JadVREZDJCendFbzVlQTU2S2damL4A==");
    }

    public async Task<Result<PrazosCNJResponse>> ConsultarPrazosAsync(
        string numeroCNJ, CancellationToken ct = default)
    {
        try
        {
            // Remove formatação para a query
            var numeroLimpo = new string(numeroCNJ.Where(char.IsDigit).ToArray());
            var tribunal = DetectarTribunal(numeroCNJ);

            var endpoint = $"/api_publica_{tribunal}/_search";
            var query = new
            {
                query = new
                {
                    match = new { numeroProcesso = numeroCNJ }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(endpoint, query, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CNJ retornou status {Status} para processo {Numero}",
                    response.StatusCode, numeroCNJ);
                return Result<PrazosCNJResponse>.Failure(
                    Error.ExternalService("CNJ", $"Status {response.StatusCode}"));
            }

            var data = await response.Content.ReadFromJsonAsync<DataJudResponse>(cancellationToken: ct);

            if (data?.Hits?.Total?.Value == 0 || data?.Hits?.Hits == null || !data.Hits.Hits.Any())
                return Result<PrazosCNJResponse>.Failure(Error.NotFound("Processo"));

            var processo = data.Hits.Hits.First().Source;
            var movimentacoes = processo?.Movimentos?
                .Select(m => new MovimentacaoCNJ(
                    m.DataHora?.ToString("dd/MM/yyyy") ?? "Data desconhecida",
                    m.Nome ?? "Sem descrição",
                    m.Codigo?.ToString() ?? "0"))
                .ToList() ?? new List<MovimentacaoCNJ>();

            // Tenta extrair próximo prazo das movimentações
            DateTime? proximoPrazo = null;
            var movPrazo = movimentacoes.FirstOrDefault(m =>
                m.Descricao.Contains("prazo", StringComparison.OrdinalIgnoreCase) ||
                m.Descricao.Contains("intimação", StringComparison.OrdinalIgnoreCase));

            if (movPrazo != null && DateTime.TryParse(movPrazo.Data, out var dataPrazo))
                proximoPrazo = dataPrazo.AddDays(15); // Prazo típico de 15 dias

            return Result<PrazosCNJResponse>.Success(
                new PrazosCNJResponse(numeroCNJ, movimentacoes.AsReadOnly(), proximoPrazo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar CNJ para processo {Numero}", numeroCNJ);
            return Result<PrazosCNJResponse>.Failure(
                Error.ExternalService("CNJ", ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<MovimentacaoCNJ>>> GetMovimentacoesAsync(
        string numeroCNJ, CancellationToken ct = default)
    {
        var result = await ConsultarPrazosAsync(numeroCNJ, ct);
        return result.Match(
            r => Result<IReadOnlyList<MovimentacaoCNJ>>.Success(r.Movimentacoes),
            error => Result<IReadOnlyList<MovimentacaoCNJ>>.Failure(error));
    }

    private static string DetectarTribunal(string numeroCNJ)
    {
        // Extrai o código do tribunal do número CNJ (posição J.TT)
        // Formato: NNNNNNN-DD.AAAA.J.TT.OOOO
        var partes = numeroCNJ.Split('.');
        if (partes.Length >= 4)
        {
            var jTT = partes[2]; // ex: "8" para TJSP
            return jTT switch
            {
                "8" => "tjsp", // Tribunal de Justiça de SP
                "7" => "trf1", // TRF1
                _ => "tjsp"   // Default
            };
        }
        return "tjsp";
    }
}

// DTOs para a API do DataJud
record DataJudResponse(
    [property: JsonPropertyName("hits")] DataJudHits? Hits
);

record DataJudHits(
    [property: JsonPropertyName("total")] DataJudTotal? Total,
    [property: JsonPropertyName("hits")] List<DataJudHit>? Hits
);

record DataJudTotal(
    [property: JsonPropertyName("value")] int Value
);

record DataJudHit(
    [property: JsonPropertyName("_source")] DataJudProcesso? Source
);

record DataJudProcesso(
    [property: JsonPropertyName("numeroProcesso")] string? NumeroProcesso,
    [property: JsonPropertyName("movimentos")] List<DataJudMovimento>? Movimentos
);

record DataJudMovimento(
    [property: JsonPropertyName("dataHora")] DateTime? DataHora,
    [property: JsonPropertyName("nome")] string? Nome,
    [property: JsonPropertyName("codigo")] int? Codigo
);
