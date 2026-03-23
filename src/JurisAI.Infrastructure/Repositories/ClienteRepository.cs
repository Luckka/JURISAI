namespace JurisAI.Infrastructure.Repositories;

using Amazon.DynamoDBv2.Model;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Interfaces.Repositories;
using JurisAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

public class ClienteRepository : IClienteRepository
{
    private readonly DynamoDbContext _context;
    private readonly ILogger<ClienteRepository> _logger;

    public ClienteRepository(DynamoDbContext context, ILogger<ClienteRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Cliente>> GetByIdAsync(string userId, string clienteId, CancellationToken ct = default)
    {
        var result = await _context.GetItemAsync($"USER#{userId}", $"CLIENTE#{clienteId}", ct);
        return result.Match(MapFromDynamo, error => Result<Cliente>.Failure(error));
    }

    public async Task<Result<IReadOnlyList<Cliente>>> GetByUserIdAsync(
        string userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _context.QueryAsync($"USER#{userId}", "CLIENTE#", ct: ct);
        return result.Match(
            items =>
            {
                var clientes = items
                    .Select(MapFromDynamo)
                    .Where(r => r.IsSuccess)
                    .Select(r => r.Value!)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .AsReadOnly();
                return Result<IReadOnlyList<Cliente>>.Success(clientes);
            },
            error => Result<IReadOnlyList<Cliente>>.Failure(error));
    }

    public async Task<Result<Cliente>> CreateAsync(Cliente cliente, CancellationToken ct = default)
    {
        var item = MapToDynamo(cliente);
        var result = await _context.PutItemAsync(item, ct);
        return result.Match(() => Result<Cliente>.Success(cliente), error => Result<Cliente>.Failure(error));
    }

    public async Task<Result<Cliente>> UpdateAsync(Cliente cliente, CancellationToken ct = default)
    {
        var item = MapToDynamo(cliente);
        var result = await _context.PutItemAsync(item, ct);
        return result.Match(() => Result<Cliente>.Success(cliente), error => Result<Cliente>.Failure(error));
    }

    public async Task<Result> DeleteAsync(string userId, string clienteId, CancellationToken ct = default)
    {
        return await _context.DeleteItemAsync($"USER#{userId}", $"CLIENTE#{clienteId}", ct);
    }

    private Dictionary<string, AttributeValue> MapToDynamo(Cliente c) => new()
    {
        ["PK"] = new AttributeValue { S = $"USER#{c.UserId}" },
        ["SK"] = new AttributeValue { S = $"CLIENTE#{c.Id}" },
        ["Id"] = new AttributeValue { S = c.Id },
        ["UserId"] = new AttributeValue { S = c.UserId },
        ["Nome"] = new AttributeValue { S = c.Nome },
        ["Documento"] = new AttributeValue { S = c.Documento.Value },
        ["Email"] = new AttributeValue { S = c.Email.Value },
        ["Telefone"] = new AttributeValue { S = c.Telefone ?? "" },
        ["Endereco"] = new AttributeValue { S = c.Endereco ?? "" },
        ["Observacoes"] = new AttributeValue { S = c.Observacoes ?? "" },
        ["Ativo"] = new AttributeValue { BOOL = c.Ativo },
        ["CreatedAt"] = new AttributeValue { S = c.CreatedAt.ToString("O") },
        ["UpdatedAt"] = new AttributeValue { S = c.UpdatedAt.ToString("O") },
        ["EntityType"] = new AttributeValue { S = "CLIENTE" }
    };

    private Result<Cliente> MapFromDynamo(Dictionary<string, AttributeValue> item)
    {
        try
        {
            var docResult = CpfCnpj.Create(item["Documento"].S);
            var emailResult = Email.Create(item["Email"].S);

            if (!docResult.IsSuccess) return Result<Cliente>.Failure(docResult.Error!);
            if (!emailResult.IsSuccess) return Result<Cliente>.Failure(emailResult.Error!);

            var tipo = typeof(Cliente);
            var cliente = (Cliente)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(tipo);

            SetProperty(cliente, "Id", item["Id"].S);
            SetProperty(cliente, "UserId", item["UserId"].S);
            SetProperty(cliente, "Nome", item["Nome"].S);
            SetProperty(cliente, "Documento", docResult.Value!);
            SetProperty(cliente, "Email", emailResult.Value!);
            SetProperty(cliente, "Telefone", string.IsNullOrEmpty(item["Telefone"].S) ? null : item["Telefone"].S);
            SetProperty(cliente, "Endereco", string.IsNullOrEmpty(item["Endereco"].S) ? null : item["Endereco"].S);
            SetProperty(cliente, "Observacoes", string.IsNullOrEmpty(item["Observacoes"].S) ? null : item["Observacoes"].S);
            SetProperty(cliente, "Ativo", item["Ativo"].BOOL);
            SetProperty(cliente, "CreatedAt", DateTime.Parse(item["CreatedAt"].S));
            SetProperty(cliente, "UpdatedAt", DateTime.Parse(item["UpdatedAt"].S));

            return Result<Cliente>.Success(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mapear cliente do DynamoDB");
            return Result<Cliente>.Failure(Error.ExternalService("DynamoDB", "Erro ao deserializar cliente"));
        }
    }

    private static void SetProperty(object obj, string name, object? value)
    {
        var prop = obj.GetType().GetProperty(name,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(obj, value);
    }
}
