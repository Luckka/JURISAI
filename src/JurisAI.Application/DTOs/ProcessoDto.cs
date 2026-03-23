namespace JurisAI.Application.DTOs;

using JurisAI.Domain.Enums;

public record ProcessoDto(
    string Id,
    string NumeroCNJ,
    string ClienteId,
    string? ClienteNome,
    string Titulo,
    TipoAcao TipoAcao,
    FaseProcessual Fase,
    StatusProcesso Status,
    string? Tribunal,
    string? Vara,
    string? JuizResponsavel,
    string? ParteAdversa,
    string? Observacoes,
    DateTime? UltimaMovimentacao,
    DateTime? ProximoPrazo,
    int? DiasParaPrazo,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
