namespace JurisAI.Infrastructure.Repositories;

using Amazon.DynamoDBv2.Model;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class HonorarioRepository : IHonorarioRepository
{
    private readonly DynamoDbContext _context;
    private readonly ILogger<HonorarioRepository> _logger;

    public HonorarioRepository(DynamoDbContext context, ILogger<HonorarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Honorario>> GetByIdAsync(string userId, string honorarioId, CancellationToken ct = default)
    {
        var result = await _context.GetItemAsync($"USER#{userId}", $"HONORARIO#{honorarioId}", ct);
        return result.Match(MapFromDynamo, error => Result<Honorario>.Failure(error));
    }

    public async Task<Result<IReadOnlyList<Honorario>>> GetByUserIdAsync(
        string userId, bool? apenasPendentes = null, CancellationToken ct = default)
    {
        var result = await _context.QueryAsync($"USER#{userId}", "HONORARIO#", ct: ct);
        return result.Match(
            items =>
            {
                var honorarios = items
                    .Select(MapFromDynamo)
                    .Where(r => r.IsSuccess)
                    .Select(r => r.Value!)
                    .Where(h => apenasPendentes == null || h.Pago != apenasPendentes.Value)
                    .ToList()
                    .AsReadOnly();
                return Result<IReadOnlyList<Honorario>>.Success(honorarios);
            },
            error => Result<IReadOnlyList<Honorario>>.Failure(error));
    }

    public async Task<Result<Honorario>> CreateAsync(Honorario honorario, CancellationToken ct = default)
    {
        var result = await _context.PutItemAsync(MapToDynamo(honorario), ct);
        return result.Match(() => Result<Honorario>.Success(honorario), error => Result<Honorario>.Failure(error));
    }

    public async Task<Result<Honorario>> UpdateAsync(Honorario honorario, CancellationToken ct = default)
    {
        var result = await _context.PutItemAsync(MapToDynamo(honorario), ct);
        return result.Match(() => Result<Honorario>.Success(honorario), error => Result<Honorario>.Failure(error));
    }

    public async Task<Result<IReadOnlyList<Honorario>>> GetByProcessoIdAsync(
        string userId, string processoId, CancellationToken ct = default)
    {
        var result = await _context.QueryAsync($"USER#{userId}", "HONORARIO#", ct: ct);
        return result.Match(
            items =>
            {
                var honorarios = items
                    .Select(MapFromDynamo)
                    .Where(r => r.IsSuccess && r.Value!.ProcessoId == processoId)
                    .Select(r => r.Value!)
                    .ToList()
                    .AsReadOnly();
                return Result<IReadOnlyList<Honorario>>.Success(honorarios);
            },
            error => Result<IReadOnlyList<Honorario>>.Failure(error));
    }

    private Dictionary<string, AttributeValue> MapToDynamo(Honorario h) => new()
    {
        ["PK"] = new AttributeValue { S = $"USER#{h.UserId}" },
        ["SK"] = new AttributeValue { S = $"HONORARIO#{h.Id}" },
        ["Id"] = new AttributeValue { S = h.Id },
        ["UserId"] = new AttributeValue { S = h.UserId },
        ["ClienteId"] = new AttributeValue { S = h.ClienteId },
        ["ProcessoId"] = new AttributeValue { S = h.ProcessoId ?? "" },
        ["Descricao"] = new AttributeValue { S = h.Descricao },
        ["Valor"] = new AttributeValue { N = h.Valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
        ["DataVencimento"] = new AttributeValue { S = h.DataVencimento.ToString("O") },
        ["DataPagamento"] = h.DataPagamento.HasValue
            ? new AttributeValue { S = h.DataPagamento.Value.ToString("O") }
            : new AttributeValue { NULL = true },
        ["Pago"] = new AttributeValue { BOOL = h.Pago },
        ["FormaPagamento"] = new AttributeValue { S = h.FormaPagamento ?? "" },
        ["Observacoes"] = new AttributeValue { S = h.Observacoes ?? "" },
        ["CreatedAt"] = new AttributeValue { S = h.CreatedAt.ToString("O") },
        ["UpdatedAt"] = new AttributeValue { S = h.UpdatedAt.ToString("O") },
        ["EntityType"] = new AttributeValue { S = "HONORARIO" }
    };

    private Result<Honorario> MapFromDynamo(Dictionary<string, AttributeValue> item)
    {
        try
        {
            var tipo = typeof(Honorario);
            var h = (Honorario)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(tipo);

            SetProperty(h, "Id", item["Id"].S);
            SetProperty(h, "UserId", item["UserId"].S);
            SetProperty(h, "ClienteId", item["ClienteId"].S);
            SetProperty(h, "ProcessoId", string.IsNullOrEmpty(item["ProcessoId"].S) ? null : item["ProcessoId"].S);
            SetProperty(h, "Descricao", item["Descricao"].S);
            SetProperty(h, "Valor", decimal.Parse(item["Valor"].N, System.Globalization.CultureInfo.InvariantCulture));
            SetProperty(h, "DataVencimento", DateTime.Parse(item["DataVencimento"].S));
            SetProperty(h, "Pago", item["Pago"].BOOL);
            SetProperty(h, "FormaPagamento", string.IsNullOrEmpty(item["FormaPagamento"].S) ? null : item["FormaPagamento"].S);
            SetProperty(h, "Observacoes", string.IsNullOrEmpty(item["Observacoes"].S) ? null : item["Observacoes"].S);
            SetProperty(h, "CreatedAt", DateTime.Parse(item["CreatedAt"].S));
            SetProperty(h, "UpdatedAt", DateTime.Parse(item["UpdatedAt"].S));

            if (item.TryGetValue("DataPagamento", out var dp) && !dp.NULL)
                SetProperty(h, "DataPagamento", DateTime.Parse(dp.S));

            return Result<Honorario>.Success(h);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mapear honorário do DynamoDB");
            return Result<Honorario>.Failure(Error.ExternalService("DynamoDB", "Erro ao deserializar honorário"));
        }
    }

    private static void SetProperty(object obj, string name, object? value)
    {
        var prop = obj.GetType().GetProperty(name,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(obj, value);
    }
}
