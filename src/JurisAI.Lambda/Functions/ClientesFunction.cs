namespace JurisAI.Lambda.Functions;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using JurisAI.Application.UseCases.Clientes.CriarCliente;
using JurisAI.Application.UseCases.Clientes.ListarClientes;
using JurisAI.Application.UseCases.Clientes.ObterCliente;
using JurisAI.Lambda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

public class ClientesFunction
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ClientesFunction()
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
            return (method, HasId(path)) switch
            {
                ("GET", false) => await ListarClientes(userId),
                ("GET", true) => await ObterCliente(userId, GetId(path)),
                ("POST", false) => await CriarCliente(userId, request.Body),
                _ => ApiResponse.NotFound()
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro não tratado em ClientesFunction: {ex}");
            return ApiResponse.InternalError();
        }
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> ListarClientes(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListarClientesHandler>();
        var result = await handler.HandleAsync(new ListarClientesQuery(userId));
        return result.Match(clientes => ApiResponse.Ok(clientes), error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> ObterCliente(string userId, string id)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ObterClienteHandler>();
        var result = await handler.HandleAsync(new ObterClienteQuery(userId, id));
        return result.Match(cliente => ApiResponse.Ok(cliente), error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> CriarCliente(string userId, string? body)
    {
        if (string.IsNullOrEmpty(body))
            return ApiResponse.BadRequest("Body é obrigatório");

        var dto = JsonSerializer.Deserialize<CriarClienteRequest>(body, JsonOptions);
        if (dto == null) return ApiResponse.BadRequest("Body inválido");

        var command = new CriarClienteCommand(
            userId, dto.Nome, dto.Documento, dto.Email,
            dto.Telefone, dto.Endereco, dto.Observacoes);

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CriarClienteHandler>();
        var result = await handler.HandleAsync(command);
        return result.Match(cliente => ApiResponse.Created(cliente), error => ApiResponse.FromError(error));
    }

    private static bool HasId(string path) =>
        path.Split('/').Any(s => s != "clientes" && !string.IsNullOrEmpty(s));

    private static string GetId(string path)
    {
        var parts = path.Split('/');
        var idx = Array.IndexOf(parts, "clientes");
        return idx >= 0 && idx + 1 < parts.Length ? parts[idx + 1] : string.Empty;
    }
}

record CriarClienteRequest(
    string Nome, string Documento, string Email,
    string? Telefone, string? Endereco, string? Observacoes);
