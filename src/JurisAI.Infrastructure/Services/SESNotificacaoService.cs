namespace JurisAI.Infrastructure.Services;

using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

public class SESNotificacaoService : INotificacaoService
{
    private readonly IAmazonSimpleEmailService _ses;
    private readonly ILogger<SESNotificacaoService> _logger;
    private const string RemetenteEmail = "noreply@jurisai.com.br";

    public SESNotificacaoService(IAmazonSimpleEmailService ses, ILogger<SESNotificacaoService> logger)
    {
        _ses = ses;
        _logger = logger;
    }

    public async Task<Result> EnviarAlertaPrazoAsync(
        string email, Prazo prazo, Processo processo, CancellationToken ct = default)
    {
        try
        {
            var urgencia = prazo.DiasRestantes <= 0 ? "VENCIDO" :
                           prazo.DiasRestantes <= 3 ? "URGENTE" : "ATENÇÃO";

            var assunto = $"[JurisAI] {urgencia} - Prazo: {prazo.Descricao}";
            var corpo = $"""
                <html>
                <body style="font-family: Arial, sans-serif; color: #333;">
                  <h2 style="color: #1B4FD8;">JurisAI — Alerta de Prazo</h2>
                  <div style="background: #f8f9fa; padding: 20px; border-radius: 8px;">
                    <p><strong>Processo:</strong> {processo.NumeroCNJ.Value}</p>
                    <p><strong>Prazo:</strong> {prazo.Descricao}</p>
                    <p><strong>Data:</strong> {prazo.DataPrazo:dd/MM/yyyy}</p>
                    <p><strong>Dias restantes:</strong> {(prazo.DiasRestantes <= 0 ? "VENCIDO" : prazo.DiasRestantes.ToString())}</p>
                  </div>
                  <p>Acesse o <a href="https://app.jurisai.com.br">JurisAI</a> para gerenciar seus processos.</p>
                </body>
                </html>
                """;

            var request = new SendEmailRequest
            {
                Source = RemetenteEmail,
                Destination = new Destination { ToAddresses = [email] },
                Message = new Message
                {
                    Subject = new Content(assunto),
                    Body = new Body { Html = new Content(corpo) }
                }
            };

            await _ses.SendEmailAsync(request, ct);
            _logger.LogInformation("Alerta de prazo enviado para {Email}", email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar alerta de prazo para {Email}", email);
            return Result.Failure(Error.ExternalService("SES", ex.Message));
        }
    }

    public async Task<Result> EnviarBoasVindasAsync(
        string email, string nomeUsuario, CancellationToken ct = default)
    {
        try
        {
            var request = new SendEmailRequest
            {
                Source = RemetenteEmail,
                Destination = new Destination { ToAddresses = [email] },
                Message = new Message
                {
                    Subject = new Content("Bem-vindo ao JurisAI!"),
                    Body = new Body
                    {
                        Html = new Content($"""
                            <html>
                            <body style="font-family: Arial, sans-serif;">
                              <h2 style="color: #1B4FD8;">Bem-vindo ao JurisAI, {nomeUsuario}!</h2>
                              <p>Sua conta foi criada com sucesso. Agora você pode:</p>
                              <ul>
                                <li>Gerenciar seus processos judiciais</li>
                                <li>Controlar honorários</li>
                                <li>Receber alertas de prazos</li>
                                <li>Gerar peças com IA</li>
                              </ul>
                              <a href="https://app.jurisai.com.br" style="background: #1B4FD8; color: white; padding: 12px 24px; border-radius: 6px; text-decoration: none;">
                                Acessar JurisAI
                              </a>
                            </body>
                            </html>
                            """)
                    }
                }
            };

            await _ses.SendEmailAsync(request, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar boas-vindas para {Email}", email);
            return Result.Failure(Error.ExternalService("SES", ex.Message));
        }
    }
}
