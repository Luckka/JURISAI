namespace JurisAI.Application.DTOs;

public record ClienteDto(
    string Id,
    string Nome,
    string Documento,
    string DocumentoFormatado,
    string Email,
    string? Telefone,
    string? Endereco,
    string? Observacoes,
    bool Ativo,
    int TotalProcessos,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
