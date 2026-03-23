namespace JurisAI.Domain.Interfaces.Services;

using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;

public interface INotificacaoService
{
    Task<Result> EnviarAlertaPrazoAsync(string email, Prazo prazo, Processo processo, CancellationToken ct = default);
    Task<Result> EnviarBoasVindasAsync(string email, string nomeUsuario, CancellationToken ct = default);
}
