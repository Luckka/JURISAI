namespace JurisAI.Lambda.Functions;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using JurisAI.Application.UseCases.Processos.CriarProcesso;
using JurisAI.Application.UseCases.Processos.ListarProcessos;
using JurisAI.Application.UseCases.Processos.ObterProcesso;
using JurisAI.Application.UseCases.Processos.AtualizarProcesso;
using JurisAI.Application.UseCases.Processos.ConsultarPrazosCNJ;
using JurisAI.Domain.Enums;
using JurisAI.Lambda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

public class ProcessosFunction
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ProcessosFunction()
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
                ("GET", false) => await ListarProcessos(userId),
                ("GET", true) => await ObterProcesso(userId, GetId(path)),
                ("POST", false) => await CriarProcesso(userId, request.Body),
                ("PUT", true) => await AtualizarProcesso(userId, GetId(path), request.Body),
                ("POST", true) when path.EndsWith("/consultar-cnj") => await ConsultarCNJ(userId, GetId(path)),
                _ => ApiResponse.NotFound()
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro não tratado: {ex}");
            return ApiResponse.InternalError();
        }
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> ListarProcessos(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ListarProcessosHandler>();
        var result = await handler.HandleAsync(new ListarProcessosQuery(userId));

        return result.Match(
            processos => ApiResponse.Ok(processos),
            error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> ObterProcesso(string userId, string id)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ObterProcessoHandler>();
        var result = await handler.HandleAsync(new ObterProcessoQuery(userId, id));

        return result.Match(
            processo => ApiResponse.Ok(processo),
            error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> CriarProcesso(string userId, string? body)
    {
        if (string.IsNullOrEmpty(body))
            return ApiResponse.BadRequest("Body é obrigatório");

        var dto = JsonSerializer.Deserialize<CriarProcessoRequest>(body, JsonOptions);
        if (dto == null) return ApiResponse.BadRequest("Body inválido");

        var command = new CriarProcessoCommand(
            userId, dto.NumeroCNJ, dto.ClienteId, dto.Titulo,
            dto.TipoAcao, dto.Fase, dto.Tribunal, dto.Vara,
            dto.JuizResponsavel, dto.ParteAdversa, dto.Observacoes);

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CriarProcessoHandler>();
        var result = await handler.HandleAsync(command);

        return result.Match(
            processo => ApiResponse.Created(processo),
            error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> AtualizarProcesso(
        string userId, string id, string? body)
    {
        if (string.IsNullOrEmpty(body))
            return ApiResponse.BadRequest("Body é obrigatório");

        var dto = JsonSerializer.Deserialize<AtualizarProcessoRequest>(body, JsonOptions);
        if (dto == null) return ApiResponse.BadRequest("Body inválido");

        var command = new AtualizarProcessoCommand(
            userId, id, dto.Titulo, dto.Fase, dto.Status,
            dto.Tribunal, dto.Vara, dto.JuizResponsavel,
            dto.ParteAdversa, dto.Observacoes);

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<AtualizarProcessoHandler>();
        var result = await handler.HandleAsync(command);

        return result.Match(
            processo => ApiResponse.Ok(processo),
            error => ApiResponse.FromError(error));
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> ConsultarCNJ(string userId, string processoId)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ConsultarPrazosCNJHandler>();
        var result = await handler.HandleAsync(new ConsultarPrazosCNJCommand(userId, processoId));

        return result.Match(
            prazos => ApiResponse.Ok(prazos),
            error => ApiResponse.FromError(error));
    }

    private static bool HasId(string path) =>
        path.Split('/').Any(s => s != "processos" && !string.IsNullOrEmpty(s) && s != "consultar-cnj");

    private static string GetId(string path)
    {
        var parts = path.Split('/');
        var idx = Array.IndexOf(parts, "processos");
        return idx >= 0 && idx + 1 < parts.Length ? parts[idx + 1] : string.Empty;
    }
}

record CriarProcessoRequest(
    string NumeroCNJ, string ClienteId, string Titulo,
    TipoAcao TipoAcao, FaseProcessual Fase,
    string? Tribunal, string? Vara, string? JuizResponsavel,
    string? ParteAdversa, string? Observacoes);

record AtualizarProcessoRequest(
    string Titulo, FaseProcessual Fase, StatusProcesso Status,
    string? Tribunal, string? Vara, string? JuizResponsavel,
    string? ParteAdversa, string? Observacoes);
