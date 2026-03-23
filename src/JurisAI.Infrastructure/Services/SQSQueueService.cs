namespace JurisAI.Infrastructure.Services;

using Amazon.SQS;
using Amazon.SQS.Model;
using JurisAI.Domain.Common;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// Serviço de fila SQS para alertas de prazo assíncronos.
/// </summary>
public class SQSQueueService
{
    private readonly IAmazonSQS _sqs;
    private readonly ILogger<SQSQueueService> _logger;

    public SQSQueueService(IAmazonSQS sqs, ILogger<SQSQueueService> logger)
    {
        _sqs = sqs;
        _logger = logger;
    }

    public async Task<Result> EnviarMensagemAsync<T>(
        string queueUrl, T mensagem, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(mensagem);
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = json
            };

            await _sqs.SendMessageAsync(request, ct);
            _logger.LogInformation("Mensagem enviada para SQS queue: {QueueUrl}", queueUrl);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem para SQS: {QueueUrl}", queueUrl);
            return Result.Failure(Error.ExternalService("SQS", ex.Message));
        }
    }
}
