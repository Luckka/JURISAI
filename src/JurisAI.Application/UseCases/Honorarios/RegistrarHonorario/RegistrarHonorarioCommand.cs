namespace JurisAI.Application.UseCases.Honorarios.RegistrarHonorario;

public record RegistrarHonorarioCommand(
    string UserId,
    string ClienteId,
    string Descricao,
    decimal Valor,
    DateTime DataVencimento,
    string? ProcessoId = null,
    string? Observacoes = null
);
