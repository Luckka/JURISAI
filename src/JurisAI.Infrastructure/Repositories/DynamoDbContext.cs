namespace JurisAI.Infrastructure.Repositories;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using JurisAI.Domain.Common;
using Microsoft.Extensions.Logging;

/// <summary>
/// Contexto DynamoDB implementando Single Table Design.
/// PK: USER#{userId} / SK: TIPO#{id}
/// </summary>
public class DynamoDbContext
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly ILogger<DynamoDbContext> _logger;
    public const string TableName = "jurisai-dev";

    public DynamoDbContext(IAmazonDynamoDB dynamoDb, ILogger<DynamoDbContext> logger)
    {
        _dynamoDb = dynamoDb;
        _logger = logger;
    }

    public async Task<Result<Dictionary<string, AttributeValue>>> GetItemAsync(
        string pk, string sk, CancellationToken ct = default)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["PK"] = new AttributeValue { S = pk },
                    ["SK"] = new AttributeValue { S = sk }
                }
            };

            var response = await _dynamoDb.GetItemAsync(request, ct);

            if (!response.IsItemSet || response.Item.Count == 0)
                return Result<Dictionary<string, AttributeValue>>.Failure(
                    Error.NotFound("Item"));

            return Result<Dictionary<string, AttributeValue>>.Success(response.Item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar item PK={PK} SK={SK}", pk, sk);
            return Result<Dictionary<string, AttributeValue>>.Failure(
                Error.ExternalService("DynamoDB", ex.Message));
        }
    }

    public async Task<Result<List<Dictionary<string, AttributeValue>>>> QueryAsync(
        string pk,
        string? skPrefix = null,
        string? indexName = null,
        CancellationToken ct = default)
    {
        try
        {
            var keyCondition = "#pk = :pk";
            var expressionNames = new Dictionary<string, string> { ["#pk"] = indexName != null ? "GSI1PK" : "PK" };
            var expressionValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = pk }
            };

            if (!string.IsNullOrEmpty(skPrefix))
            {
                keyCondition += " AND begins_with(#sk, :sk)";
                expressionNames["#sk"] = indexName != null ? "GSI1SK" : "SK";
                expressionValues[":sk"] = new AttributeValue { S = skPrefix };
            }

            var request = new QueryRequest
            {
                TableName = TableName,
                IndexName = indexName,
                KeyConditionExpression = keyCondition,
                ExpressionAttributeNames = expressionNames,
                ExpressionAttributeValues = expressionValues
            };

            var response = await _dynamoDb.QueryAsync(request, ct);
            return Result<List<Dictionary<string, AttributeValue>>>.Success(response.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar DynamoDB PK={PK}", pk);
            return Result<List<Dictionary<string, AttributeValue>>>.Failure(
                Error.ExternalService("DynamoDB", ex.Message));
        }
    }

    public async Task<Result> PutItemAsync(
        Dictionary<string, AttributeValue> item,
        CancellationToken ct = default)
    {
        try
        {
            var request = new PutItemRequest
            {
                TableName = TableName,
                Item = item
            };

            await _dynamoDb.PutItemAsync(request, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar item no DynamoDB");
            return Result.Failure(Error.ExternalService("DynamoDB", ex.Message));
        }
    }

    public async Task<Result> DeleteItemAsync(
        string pk, string sk, CancellationToken ct = default)
    {
        try
        {
            var request = new DeleteItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["PK"] = new AttributeValue { S = pk },
                    ["SK"] = new AttributeValue { S = sk }
                }
            };

            await _dynamoDb.DeleteItemAsync(request, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar item PK={PK} SK={SK}", pk, sk);
            return Result.Failure(Error.ExternalService("DynamoDB", ex.Message));
        }
    }
}
