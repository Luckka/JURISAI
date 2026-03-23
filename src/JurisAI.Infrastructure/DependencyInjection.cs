namespace JurisAI.Infrastructure;

using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SimpleEmail;
using Anthropic.SDK;
using JurisAI.Domain.Interfaces.Repositories;
using JurisAI.Domain.Interfaces.Services;
using JurisAI.Infrastructure.Repositories;
using JurisAI.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // AWS SDK
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IAmazonS3, AmazonS3Client>();
        services.AddSingleton<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();

        // Anthropic Claude
        services.AddSingleton(new AnthropicClient(
            configuration["ANTHROPIC_API_KEY"] ?? throw new InvalidOperationException("ANTHROPIC_API_KEY não configurada")));

        // S3 Options
        services.Configure<S3Options>(options =>
        {
            options.BucketName = configuration["S3_BUCKET_NAME"] ?? "jurisai-documentos";
        });

        // DynamoDB Context
        services.AddScoped<DynamoDbContext>();

        // Repositories
        services.AddScoped<IProcessoRepository, ProcessoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IHonorarioRepository, HonorarioRepository>();
        services.AddScoped<IPecaRepository, PecaRepository>();

        // Services
        services.AddScoped<IIAService, IAService>();
        services.AddScoped<IStorageService, S3StorageService>();
        services.AddScoped<INotificacaoService, SESNotificacaoService>();

        // HTTP Client para CNJ
        services.AddHttpClient<ICNJService, CNJService>();

        return services;
    }
}
