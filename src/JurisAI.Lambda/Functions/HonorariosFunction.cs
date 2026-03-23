namespace JurisAI.Lambda.Functions;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using JurisAI.Application.UseCases.Honorarios.ListarHonorarios;
using JurisAI.Application.UseCases.Honorarios.MarcarComoPago;
using JurisAI.Application.UseCases.Honorarios.RegistrarHonorario;
using JurisAI.Lambda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

public class HonorariosFunction
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public HonorariosFunction()
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
        var isPagar = path.EndsWith("/pagar");

        try
        {
            return (method, HasId(path), isPagar) switch
            {
                ("GET", false, false) => await ListarHonorarios(userId, request.QueryStringParameters),
                ("POST", false, false) => await RegistrarHonorario(userId, request.Body),
                ("PUT", true, true) => await MarcarComoPago(userId, GetId(path), request.Body),
                _ => ApiResponse.NotFound()
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro não tratado em HonorariosFunction: {ex}");
            return ApiResponse.InternalError();
        }
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> ListarHonorarios(
        string userId, IDictionary<string, string>? queryParams)
    {
        bool? apenasPendentes = null;
        if (queryParams != null && queryParams.TryGetValue("apenasPendentes", out var val))
            apenasPendentes = val == "true";

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListarHonorariosHandler>();
        var result = await handler.HandleAsync(new ListarHonorariosQuery(userId, apenasPendentes));
        return result.Match(honorarios => ApiResponse.Ok(honorarios), error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> RegistrarHonorario(string userId, string? body)
    {
        if (string.IsNullOrEmpty(body))
            return ApiResponse.BadRequest("Body é obrigatório");

        var dto = JsonSerializer.Deserialize<RegistrarHonorarioRequest>(body, JsonOptions);
        if (dto == null) return ApiResponse.BadRequest("Body inválido");

        var command = new RegistrarHonorarioCommand(
            userId, dto.ClienteId, dto.Descricao, dto.Valor,
            dto.DataVencimento, dto.ProcessoId, dto.Observacoes);

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<RegistrarHonorarioHandler>();
        var result = await handler.HandleAsync(command);
        return result.Match(h => ApiResponse.Created(h), error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> MarcarComoPago(
        string userId, string honorarioId, string? body)
    {
        if (string.IsNullOrEmpty(body))
            return ApiResponse.BadRequest("Body é obrigatório");

        var dto = JsonSerializer.Deserialize<PagarHonorarioRequest>(body, JsonOptions);
        if (dto == null) return ApiResponse.BadRequest("Body inválido");

        var command = new MarcarComoPagoCommand(userId, honorarioId, dto.FormaPagamento);

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<MarcarComoPagoHandler>();
        var result = await handler.HandleAsync(command);
        return result.Match(h => ApiResponse.Ok(h), error => ApiResponse.FromError(error));
    }

    private static bool HasId(string path) =>
        path.Split('/').Any(s => s != "honorarios" && s != "pagar" && !string.IsNullOrEmpty(s));

    private static string GetId(string path)
    {
        var parts = path.Split('/');
        var idx = Array.IndexOf(parts, "honorarios");
        return idx >= 0 && idx + 1 < parts.Length ? parts[idx + 1] : string.Empty;
    }
}

record RegistrarHonorarioRequest(
    string ClienteId, string Descricao, decimal Valor,
    DateTime DataVencimento, string? ProcessoId, string? Observacoes);

record PagarHonorarioRequest(string FormaPagamento);
