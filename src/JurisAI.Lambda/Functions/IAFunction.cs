namespace JurisAI.Lambda.Functions;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using JurisAI.Application.UseCases.IA.GerarPeca;
using JurisAI.Application.UseCases.IA.ListarPecas;
using JurisAI.Lambda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

public class IAFunction
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public IAFunction()
    {
        _serviceProvider = LambdaStartup.BuildServiceProvider();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
        APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var userId = AuthMiddleware.GetUserId(request);
        if (string.IsNullOrEmpty(userId))
            return ApiResponse.Unauthorized();

        var method = request.RequestContext.Http.Method.ToUpperInvariant();
        var path = request.RequestContext.Http.Path;

        try
        {
            if (method == "POST" && path.EndsWith("/gerar-peca"))
                return await GerarPeca(userId, request.Body);

            if (method == "GET" && path.EndsWith("/pecas"))
                return await ListarPecas(userId, request.QueryStringParameters);

            return ApiResponse.NotFound();
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro não tratado em IAFunction: {ex}");
            return ApiResponse.InternalError();
        }
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> GerarPeca(string userId, string? body)
    {
        if (string.IsNullOrEmpty(body))
            return ApiResponse.BadRequest("Body é obrigatório");

        var dto = JsonSerializer.Deserialize<GerarPecaRequest>(body, JsonOptions);
        if (dto == null) return ApiResponse.BadRequest("Body inválido");

        var command = new GerarPecaCommand(userId, dto.TipoPeca, dto.Contexto, dto.ProcessoId);

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GerarPecaHandler>();
        var result = await handler.HandleAsync(command);
        return result.Match(peca => ApiResponse.Created(peca), error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> ListarPecas(
        string userId, IDictionary<string, string>? queryParams)
    {
        string? processoId = null;
        queryParams?.TryGetValue("processoId", out processoId);

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListarPecasHandler>();
        var result = await handler.HandleAsync(new ListarPecasQuery(userId, processoId));
        return result.Match(pecas => ApiResponse.Ok(pecas), error => ApiResponse.FromError(error));
    }
}

record GerarPecaRequest(string TipoPeca, string Contexto, string? ProcessoId);
