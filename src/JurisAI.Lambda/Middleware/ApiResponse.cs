namespace JurisAI.Lambda.Middleware;

using Amazon.Lambda.APIGatewayEvents;
using JurisAI.Domain.Common;
using System.Text.Json;

public static class ApiResponse
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static APIGatewayHttpApiV2ProxyResponse Json(int statusCode, object body) => new()
    {
        StatusCode = statusCode,
        Body = JsonSerializer.Serialize(body, Options),
        Headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["Access-Control-Allow-Origin"] = "*",
            ["Access-Control-Allow-Headers"] = "Content-Type,Authorization",
            ["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS"
        }
    };

    public static APIGatewayHttpApiV2ProxyResponse Ok(object data) =>
        Json(200, new { success = true, data });

    public static APIGatewayHttpApiV2ProxyResponse Created(object data) =>
        Json(201, new { success = true, data });

    public static APIGatewayHttpApiV2ProxyResponse BadRequest(string message) =>
        Json(400, new { success = false, error = new { code = "BadRequest", message } });

    public static APIGatewayHttpApiV2ProxyResponse Unauthorized() =>
        Json(401, new { success = false, error = new { code = "Unauthorized", message = "Não autorizado." } });

    public static APIGatewayHttpApiV2ProxyResponse NotFound() =>
        Json(404, new { success = false, error = new { code = "NotFound", message = "Recurso não encontrado." } });

    public static APIGatewayHttpApiV2ProxyResponse InternalError() =>
        Json(500, new { success = false, error = new { code = "InternalError", message = "Erro interno do servidor." } });

    public static APIGatewayHttpApiV2ProxyResponse FromError(Error error) =>
        error.Code switch
        {
            var c when c.EndsWith(".NotFound") => Json(404, new { success = false, error }),
            "Auth.Unauthorized" => Json(401, new { success = false, error }),
            "Validation.Error" => Json(400, new { success = false, error }),
            "Conflict.Error" => Json(409, new { success = false, error }),
            var c when c.EndsWith(".LimitExceeded") => Json(402, new { success = false, error }),
            _ => Json(500, new { success = false, error })
        };
}
