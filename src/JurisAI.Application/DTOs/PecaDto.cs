namespace JurisAI.Application.DTOs;

public record PecaDto(
    string Id,
    string? ProcessoId,
    string Titulo,
    string TipoPeca,
    string Conteudo,
    string? S3Key,
    bool GeradaPorIA,
    string? ModeloIA,
    int? TokensUtilizados,
    DateTime CreatedAt
);
