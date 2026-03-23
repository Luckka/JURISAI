namespace JurisAI.Domain.Common;

/// <summary>
/// Representa um erro de domínio com código e mensagem descritiva.
/// </summary>
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity) =>
        new($"{entity}.NotFound", $"{entity} não encontrado.");

    public static Error Validation(string message) =>
        new("Validation.Error", message);

    public static Error Unauthorized() =>
        new("Auth.Unauthorized", "Não autorizado.");

    public static Error LimitExceeded(string resource) =>
        new($"{resource}.LimitExceeded", $"Limite do plano atingido para {resource}.");

    public static Error ExternalService(string service, string detail) =>
        new($"{service}.Error", $"Erro no serviço {service}: {detail}");

    public static Error Conflict(string message) =>
        new("Conflict.Error", message);
}
