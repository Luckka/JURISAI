namespace JurisAI.Lambda.Middleware;

using JurisAI.Application.UseCases.Processos.CriarProcesso;
using JurisAI.Application.UseCases.Processos.ListarProcessos;
using JurisAI.Application.UseCases.Processos.ObterProcesso;
using JurisAI.Application.UseCases.Processos.AtualizarProcesso;
using JurisAI.Application.UseCases.Processos.ConsultarPrazosCNJ;
using JurisAI.Application.UseCases.Clientes.CriarCliente;
using JurisAI.Application.UseCases.Clientes.ListarClientes;
using JurisAI.Application.UseCases.Clientes.ObterCliente;
using JurisAI.Application.UseCases.Honorarios.RegistrarHonorario;
using JurisAI.Application.UseCases.Honorarios.ListarHonorarios;
using JurisAI.Application.UseCases.Honorarios.MarcarComoPago;
using JurisAI.Application.UseCases.IA.GerarPeca;
using JurisAI.Application.UseCases.IA.ListarPecas;
using JurisAI.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class LambdaStartup
{
    public static IServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        // Infrastructure (repositórios, serviços AWS, IA)
        services.AddInfrastructure(configuration);

        // Application handlers — Processos
        services.AddScoped<CriarProcessoHandler>();
        services.AddScoped<ListarProcessosHandler>();
        services.AddScoped<ObterProcessoHandler>();
        services.AddScoped<AtualizarProcessoHandler>();
        services.AddScoped<ConsultarPrazosCNJHandler>();

        // Application handlers — Clientes
        services.AddScoped<CriarClienteHandler>();
        services.AddScoped<ListarClientesHandler>();
        services.AddScoped<ObterClienteHandler>();

        // Application handlers — Honorários
        services.AddScoped<RegistrarHonorarioHandler>();
        services.AddScoped<ListarHonorariosHandler>();
        services.AddScoped<MarcarComoPagoHandler>();

        // Application handlers — IA
        services.AddScoped<GerarPecaHandler>();
        services.AddScoped<ListarPecasHandler>();

        return services.BuildServiceProvider();
    }
}
