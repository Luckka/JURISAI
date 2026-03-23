namespace JurisAI.Infrastructure.Repositories;

using Amazon.DynamoDBv2.Model;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class PecaRepository : IPecaRepository
{
    private readonly DynamoDbContext _context;
    private readonly ILogger<PecaRepository> _logger;

    public PecaRepository(DynamoDbContext context, ILogger<PecaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Peca>> GetByIdAsync(string userId, string pecaId, CancellationToken ct = default)
    {
        var result = await _context.GetItemAsync($"USER#{userId}", $"PECA#{pecaId}", ct);
        return result.Match(MapFromDynamo, error => Result<Peca>.Failure(error));
    }

    public async Task<Result<IReadOnlyList<Peca>>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        var result = await _context.QueryAsync($"USER#{userId}", "PECA#", ct: ct);
        return result.Match(
            items => Result<IReadOnlyList<Peca>>.Success(
                items.Select(MapFromDynamo).Where(r => r.IsSuccess).Select(r => r.Value!).ToList().AsReadOnly()),
            error => Result<IReadOnlyList<Peca>>.Failure(error));
    }

    public async Task<Result<Peca>> CreateAsync(Peca peca, CancellationToken ct = default)
    {
        var result = await _context.PutItemAsync(MapToDynamo(peca), ct);
        return result.Match(() => Result<Peca>.Success(peca), error => Result<Peca>.Failure(error));
    }

    public async Task<Result<IReadOnlyList<Peca>>> GetByProcessoIdAsync(
        string userId, string processoId, CancellationToken ct = default)
    {
        var result = await _context.QueryAsync($"USER#{userId}", "PECA#", ct: ct);
        return result.Match(
            items => Result<IReadOnlyList<Peca>>.Success(
                items.Select(MapFromDynamo)
                    .Where(r => r.IsSuccess && r.Value!.ProcessoId == processoId)
                    .Select(r => r.Value!)
                    .ToList().AsReadOnly()),
            error => Result<IReadOnlyList<Peca>>.Failure(error));
    }

    private Dictionary<string, AttributeValue> MapToDynamo(Peca p) => new()
    {
        ["PK"] = new AttributeValue { S = $"USER#{p.UserId}" },
        ["SK"] = new AttributeValue { S = $"PECA#{p.Id}" },
        ["Id"] = new AttributeValue { S = p.Id },
        ["UserId"] = new AttributeValue { S = p.UserId },
        ["ProcessoId"] = new AttributeValue { S = p.ProcessoId ?? "" },
        ["Titulo"] = new AttributeValue { S = p.Titulo },
        ["TipoPeca"] = new AttributeValue { S = p.TipoPeca },
        ["Conteudo"] = new AttributeValue { S = p.Conteudo },
        ["S3Key"] = new AttributeValue { S = p.S3Key ?? "" },
        ["GeradaPorIA"] = new AttributeValue { BOOL = p.GeradaPorIA },
        ["PromptUtilizado"] = new AttributeValue { S = p.PromptUtilizado ?? "" },
        ["ModeloIA"] = new AttributeValue { S = p.ModeloIA ?? "" },
        ["TokensUtilizados"] = p.TokensUtilizados.HasValue
            ? new AttributeValue { N = p.TokensUtilizados.Value.ToString() }
            : new AttributeValue { NULL = true },
        ["CreatedAt"] = new AttributeValue { S = p.CreatedAt.ToString("O") },
        ["UpdatedAt"] = new AttributeValue { S = p.UpdatedAt.ToString("O") },
        ["EntityType"] = new AttributeValue { S = "PECA" }
    };

    private Result<Peca> MapFromDynamo(Dictionary<string, AttributeValue> item)
    {
        try
        {
            var tipo = typeof(Peca);
            var p = (Peca)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(tipo);

            SetProperty(p, "Id", item["Id"].S);
            SetProperty(p, "UserId", item["UserId"].S);
            SetProperty(p, "ProcessoId", string.IsNullOrEmpty(item["ProcessoId"].S) ? null : item["ProcessoId"].S);
            SetProperty(p, "Titulo", item["Titulo"].S);
            SetProperty(p, "TipoPeca", item["TipoPeca"].S);
            SetProperty(p, "Conteudo", item["Conteudo"].S);
            SetProperty(p, "S3Key", string.IsNullOrEmpty(item["S3Key"].S) ? null : item["S3Key"].S);
            SetProperty(p, "GeradaPorIA", item["GeradaPorIA"].BOOL);
            SetProperty(p, "PromptUtilizado", string.IsNullOrEmpty(item["PromptUtilizado"].S) ? null : item["PromptUtilizado"].S);
            SetProperty(p, "ModeloIA", string.IsNullOrEmpty(item["ModeloIA"].S) ? null : item["ModeloIA"].S);
            SetProperty(p, "CreatedAt", DateTime.Parse(item["CreatedAt"].S));
            SetProperty(p, "UpdatedAt", DateTime.Parse(item["UpdatedAt"].S));

            if (item.TryGetValue("TokensUtilizados", out var tu) && !tu.NULL)
                SetProperty(p, "TokensUtilizados", int.Parse(tu.N));

            return Result<Peca>.Success(p);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mapear peça do DynamoDB");
            return Result<Peca>.Failure(Error.ExternalService("DynamoDB", "Erro ao deserializar peça"));
        }
    }

    private static void SetProperty(object obj, string name, object? value)
    {
        var prop = obj.GetType().GetProperty(name,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(obj, value);
    }
}
