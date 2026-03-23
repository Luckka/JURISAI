namespace JurisAI.Application.UseCases.Processos.AtualizarProcesso;

using JurisAI.Domain.Enums;

public record AtualizarProcessoCommand(
    string UserId,
    string ProcessoId,
    string Titulo,
    FaseProcessual Fase,
    StatusProcesso Status,
    string? Tribunal = null,
    string? Vara = null,
    string? JuizResponsavel = null,
    string? ParteAdversa = null,
    string? Observacoes = null
);
