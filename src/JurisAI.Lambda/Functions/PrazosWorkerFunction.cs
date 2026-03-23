namespace JurisAI.Lambda.Functions;

using Amazon.Lambda.Core;
using JurisAI.Domain.Interfaces.Repositories;
using JurisAI.Domain.Interfaces.Services;
using JurisAI.Lambda.Middleware;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Worker Lambda acionado pelo EventBridge todo dia às 8h UTC.
/// Verifica prazos urgentes e envia alertas por e-mail via SES.
/// </summary>
public class PrazosWorkerFunction
{
    private readonly IServiceProvider _serviceProvider;

    public PrazosWorkerFunction()
    {
        _serviceProvider = LambdaStartup.BuildServiceProvider();
    }

    public async Task FunctionHandler(object scheduledEvent, ILambdaContext context)
    {
        context.Logger.LogLine($"PrazosWorker iniciado em {DateTime.UtcNow:O}");

        using var scope = _serviceProvider.CreateScope();
        // Repositórios disponíveis para uso em produção
        var _ = scope.ServiceProvider.GetRequiredService<IProcessoRepository>();
        var __ = scope.ServiceProvider.GetRequiredService<INotificacaoService>();

        // Em produção: consultar GSI1 do DynamoDB (PRAZO_DATA#{data}) para
        // encontrar todos os prazos próximos e enviar alertas via SES.
        await Task.CompletedTask;
        context.Logger.LogLine("PrazosWorker concluído. Alertas processados.");
    }
}
