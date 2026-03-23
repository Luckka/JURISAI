namespace JurisAI.Infrastructure.Services;

using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text;
using DomainError = JurisAI.Domain.Common.Error;

/// <summary>
/// Serviço de IA usando Claude claude-sonnet-4-6 com suporte a streaming.
/// Compatível com Anthropic.SDK 4.x.
/// </summary>
public class IAService : IIAService
{
    private readonly AnthropicClient _client;
    private readonly ILogger<IAService> _logger;
    private const string ModeloId = "claude-sonnet-4-6";

    private const string SystemPromptText = """
        Você é um assistente jurídico especializado em direito brasileiro.
        Você ajuda advogados a redigir peças processuais com qualidade e precisão.

        Diretrizes:
        - Utilize linguagem jurídica formal e técnica conforme o direito brasileiro
        - Siga o Código de Processo Civil (CPC/2015) e legislação vigente
        - Estruture as peças com: qualificação das partes, fatos, fundamentos jurídicos, pedidos
        - Cite artigos de lei e jurisprudência quando relevante
        - Adapte o conteúdo ao contexto específico do caso fornecido
        - Mantenha objetividade e clareza na argumentação
        - Inclua os requisitos formais de cada tipo de peça
        """;

    public IAService(AnthropicClient client, ILogger<IAService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GerarPecaResponse>> GerarPecaAsync(
        GerarPecaRequest request, CancellationToken ct = default)
    {
        try
        {
            var prompt = BuildPrompt(request);

            var parameters = new MessageParameters
            {
                Messages = [new Message(RoleType.User, prompt)],
                MaxTokens = 4096,
                Model = ModeloId,
                Stream = false,
                SystemMessage = SystemPromptText
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters, ct);

            var conteudo = response.Message?.ToString() ?? string.Empty;
            var inputTokens = response.Usage?.InputTokens ?? 0;
            var outputTokens = response.Usage?.OutputTokens ?? 0;

            _logger.LogInformation("Peça gerada com {InputTokens} input + {OutputTokens} output tokens",
                inputTokens, outputTokens);

            return Result<GerarPecaResponse>.Success(new GerarPecaResponse(
                conteudo, ModeloId, inputTokens + outputTokens));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar peça com IA");
            return Result<GerarPecaResponse>.Failure(
                DomainError.ExternalService("Claude", ex.Message));
        }
    }

    public async IAsyncEnumerable<string> GerarPecaStreamAsync(
        GerarPecaRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var prompt = BuildPrompt(request);

        var parameters = new MessageParameters
        {
            Messages = [new Message(RoleType.User, prompt)],
            MaxTokens = 4096,
            Model = ModeloId,
            Stream = true,
            SystemMessage = SystemPromptText
        };

        await foreach (var res in _client.Messages.StreamClaudeMessageAsync(parameters, ct))
        {
            if (res.Delta?.Text is { } text && !string.IsNullOrEmpty(text))
                yield return text;
        }
    }

    private static string BuildPrompt(GerarPecaRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Por favor, redija uma {request.TipoPeca} com base nas seguintes informações:");
        sb.AppendLine();
        sb.AppendLine("CONTEXTO DO CASO:");
        sb.AppendLine(request.Contexto);

        if (!string.IsNullOrEmpty(request.ProcessoNumero))
        {
            sb.AppendLine();
            sb.AppendLine($"NÚMERO DO PROCESSO: {request.ProcessoNumero}");
        }

        if (!string.IsNullOrEmpty(request.ParteAdversa))
            sb.AppendLine($"PARTE ADVERSA: {request.ParteAdversa}");

        sb.AppendLine();
        sb.AppendLine("Gere a peça completa, formal e tecnicamente correta.");

        return sb.ToString();
    }
}
