namespace JurisAI.Application.DTOs;

public record HonorarioDto(
    string Id,
    string ClienteId,
    string? ClienteNome,
    string? ProcessoId,
    string? ProcessoNumero,
    string Descricao,
    decimal Valor,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    bool Pago,
    string? FormaPagamento,
    string? Observacoes,
    DateTime CreatedAt
);
