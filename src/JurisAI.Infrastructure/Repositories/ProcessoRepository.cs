namespace JurisAI.Infrastructure.Repositories;

using Amazon.DynamoDBv2.Model;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Enums;
using JurisAI.Domain.Interfaces.Repositories;
using JurisAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

/// <summary>
/// Repositório de processos usando DynamoDB Single Table Design.
/// PK: USER#{userId}, SK: PROCESSO#{processoId}
/// </summary>
public class ProcessoRepository : IProcessoRepository
{
    private readonly DynamoDbContext _context;
    private readonly ILogger<ProcessoRepository> _logger;

    public ProcessoRepository(DynamoDbContext context, ILogger<ProcessoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Processo>> GetByIdAsync(string userId, string processoId, CancellationToken ct = default)
    {
        var result = await _context.GetItemAsync(
            $"USER#{userId}", $"PROCESSO#{processoId}", ct);

        return result.Match(
            item => MapFromDynamo(item),
            error => Result<Processo>.Failure(error));
    }

    public async Task<Result<IReadOnlyList<Processo>>> GetByUserIdAsync(
        string userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _context.QueryAsync($"USER#{userId}", "PROCESSO#", ct: ct);

        return result.Match(
            items =>
            {
                var processos = items
                    .Select(item => MapFromDynamo(item))
                    .Where(r => r.IsSuccess)
                    .Select(r => r.Value!)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .AsReadOnly();

                return Result<IReadOnlyList<Processo>>.Success(processos);
            },
            error => Result<IReadOnlyList<Processo>>.Failure(error));
    }

    public async Task<Result<Processo>> CreateAsync(Processo processo, CancellationToken ct = default)
    {
        var item = MapToDynamo(processo);
        var result = await _context.PutItemAsync(item, ct);

        return result.Match(
            () => Result<Processo>.Success(processo),
            error => Result<Processo>.Failure(error));
    }

    public async Task<Result<Processo>> UpdateAsync(Processo processo, CancellationToken ct = default)
    {
        var item = MapToDynamo(processo);
        var result = await _context.PutItemAsync(item, ct);

        return result.Match(
            () => Result<Processo>.Success(processo),
            error => Result<Processo>.Failure(error));
    }

    public async Task<Result> DeleteAsync(string userId, string processoId, CancellationToken ct = default)
    {
        return await _context.DeleteItemAsync($"USER#{userId}", $"PROCESSO#{processoId}", ct);
    }

    public async Task<Result<IReadOnlyList<Processo>>> GetByClienteIdAsync(
        string userId, string clienteId, CancellationToken ct = default)
    {
        // Busca todos os processos e filtra por clienteId (poderia usar GSI em produção)
        var result = await _context.QueryAsync($"USER#{userId}", "PROCESSO#", ct: ct);

        return result.Match(
            items =>
            {
                var processos = items
                    .Select(item => MapFromDynamo(item))
                    .Where(r => r.IsSuccess && r.Value!.ClienteId == clienteId)
                    .Select(r => r.Value!)
                    .ToList()
                    .AsReadOnly();

                return Result<IReadOnlyList<Processo>>.Success(processos);
            },
            error => Result<IReadOnlyList<Processo>>.Failure(error));
    }

    private Dictionary<string, AttributeValue> MapToDynamo(Processo p) => new()
    {
        ["PK"] = new AttributeValue { S = $"USER#{p.UserId}" },
        ["SK"] = new AttributeValue { S = $"PROCESSO#{p.Id}" },
        ["Id"] = new AttributeValue { S = p.Id },
        ["UserId"] = new AttributeValue { S = p.UserId },
        ["NumeroCNJ"] = new AttributeValue { S = p.NumeroCNJ.Value },
        ["ClienteId"] = new AttributeValue { S = p.ClienteId },
        ["Titulo"] = new AttributeValue { S = p.Titulo },
        ["TipoAcao"] = new AttributeValue { S = p.TipoAcao.ToString() },
        ["Fase"] = new AttributeValue { S = p.Fase.ToString() },
        ["Status"] = new AttributeValue { S = p.Status.ToString() },
        ["Tribunal"] = new AttributeValue { S = p.Tribunal ?? "" },
        ["Vara"] = new AttributeValue { S = p.Vara ?? "" },
        ["JuizResponsavel"] = new AttributeValue { S = p.JuizResponsavel ?? "" },
        ["ParteAdversa"] = new AttributeValue { S = p.ParteAdversa ?? "" },
        ["Observacoes"] = new AttributeValue { S = p.Observacoes ?? "" },
        ["UltimaMovimentacao"] = p.UltimaMovimentacao.HasValue
            ? new AttributeValue { S = p.UltimaMovimentacao.Value.ToString("O") }
            : new AttributeValue { NULL = true },
        ["ProximoPrazo"] = p.ProximoPrazo.HasValue
            ? new AttributeValue { S = p.ProximoPrazo.Value.ToString("O") }
            : new AttributeValue { NULL = true },
        ["CreatedAt"] = new AttributeValue { S = p.CreatedAt.ToString("O") },
        ["UpdatedAt"] = new AttributeValue { S = p.UpdatedAt.ToString("O") },
        ["EntityType"] = new AttributeValue { S = "PROCESSO" }
    };

    private Result<Processo> MapFromDynamo(Dictionary<string, AttributeValue> item)
    {
        try
        {
            var numeroCNJResult = NumeroCNJ.Create(item["NumeroCNJ"].S);
            if (!numeroCNJResult.IsSuccess)
                return Result<Processo>.Failure(numeroCNJResult.Error!);

            // Usando reflexão para criar processo sem construtor público
            var tipo = typeof(Processo);
            var processo = (Processo)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(tipo);

            SetProperty(processo, "Id", item["Id"].S);
            SetProperty(processo, "UserId", item["UserId"].S);
            SetProperty(processo, "NumeroCNJ", numeroCNJResult.Value!);
            SetProperty(processo, "ClienteId", item["ClienteId"].S);
            SetProperty(processo, "Titulo", item["Titulo"].S);
            SetProperty(processo, "TipoAcao", Enum.Parse<TipoAcao>(item["TipoAcao"].S));
            SetProperty(processo, "Fase", Enum.Parse<FaseProcessual>(item["Fase"].S));
            SetProperty(processo, "Status", Enum.Parse<StatusProcesso>(item["Status"].S));
            SetProperty(processo, "Tribunal", string.IsNullOrEmpty(item["Tribunal"].S) ? null : item["Tribunal"].S);
            SetProperty(processo, "Vara", string.IsNullOrEmpty(item["Vara"].S) ? null : item["Vara"].S);
            SetProperty(processo, "JuizResponsavel", string.IsNullOrEmpty(item["JuizResponsavel"].S) ? null : item["JuizResponsavel"].S);
            SetProperty(processo, "ParteAdversa", string.IsNullOrEmpty(item["ParteAdversa"].S) ? null : item["ParteAdversa"].S);
            SetProperty(processo, "Observacoes", string.IsNullOrEmpty(item["Observacoes"].S) ? null : item["Observacoes"].S);
            SetProperty(processo, "CreatedAt", DateTime.Parse(item["CreatedAt"].S));
            SetProperty(processo, "UpdatedAt", DateTime.Parse(item["UpdatedAt"].S));

            if (item.TryGetValue("UltimaMovimentacao", out var um) && !um.NULL)
                SetProperty(processo, "UltimaMovimentacao", DateTime.Parse(um.S));
            if (item.TryGetValue("ProximoPrazo", out var pp) && !pp.NULL)
                SetProperty(processo, "ProximoPrazo", DateTime.Parse(pp.S));

            return Result<Processo>.Success(processo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mapear processo do DynamoDB");
            return Result<Processo>.Failure(Error.ExternalService("DynamoDB", "Erro ao deserializar processo"));
        }
    }

    private static void SetProperty(object obj, string name, object? value)
    {
        var prop = obj.GetType().GetProperty(name,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(obj, value);
    }
}
