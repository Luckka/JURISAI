namespace JurisAI.Application.UseCases.Processos.CriarProcesso;

using JurisAI.Domain.Enums;

public record CriarProcessoCommand(
    string UserId,
    string NumeroCNJ,
    string ClienteId,
    string Titulo,
    TipoAcao TipoAcao,
    FaseProcessual Fase,
    string? Tribunal = null,
    string? Vara = null,
    string? JuizResponsavel = null,
    string? ParteAdversa = null,
    string? Observacoes = null
);
