namespace JurisAI.Application.UseCases.IA.GerarPeca;

public record GerarPecaCommand(
    string UserId,
    string TipoPeca,
    string Contexto,
    string? ProcessoId = null,
    bool SaveToStorage = true
);
