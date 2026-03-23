namespace JurisAI.Domain.Entities;

using JurisAI.Domain.Common;
using JurisAI.Domain.Enums;
using JurisAI.Domain.ValueObjects;

/// <summary>
/// Entidade central do sistema — representa um processo judicial.
/// </summary>
public class Processo : Entity
{
    public string UserId { get; private set; }
    public NumeroCNJ NumeroCNJ { get; private set; }
    public string ClienteId { get; private set; }
    public string Titulo { get; private set; }
    public TipoAcao TipoAcao { get; private set; }
    public FaseProcessual Fase { get; private set; }
    public StatusProcesso Status { get; private set; }
    public string? Tribunal { get; private set; }
    public string? Vara { get; private set; }
    public string? JuizResponsavel { get; private set; }
    public string? ParteAdversa { get; private set; }
    public string? Observacoes { get; private set; }
    public DateTime? UltimaMovimentacao { get; private set; }
    public DateTime? ProximoPrazo { get; private set; }

    private Processo()
    {
        UserId = null!; NumeroCNJ = null!; ClienteId = null!; Titulo = null!;
    }

    public static Result<Processo> Criar(
        string userId,
        string numeroCNJ,
        string clienteId,
        string titulo,
        TipoAcao tipoAcao,
        FaseProcessual fase,
        string? tribunal = null,
        string? vara = null,
        string? juizResponsavel = null,
        string? parteAdversa = null,
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<Processo>.Failure(Error.Validation("UserId é obrigatório."));

        if (string.IsNullOrWhiteSpace(clienteId))
            return Result<Processo>.Failure(Error.Validation("ClienteId é obrigatório."));

        if (string.IsNullOrWhiteSpace(titulo) || titulo.Length < 3)
            return Result<Processo>.Failure(Error.Validation("Título deve ter pelo menos 3 caracteres."));

        var numeroCNJResult = NumeroCNJ.Create(numeroCNJ);
        if (!numeroCNJResult.IsSuccess)
            return Result<Processo>.Failure(numeroCNJResult.Error!);

        var processo = new Processo
        {
            UserId = userId,
            NumeroCNJ = numeroCNJResult.Value!,
            ClienteId = clienteId,
            Titulo = titulo.Trim(),
            TipoAcao = tipoAcao,
            Fase = fase,
            Status = StatusProcesso.Ativo,
            Tribunal = tribunal?.Trim(),
            Vara = vara?.Trim(),
            JuizResponsavel = juizResponsavel?.Trim(),
            ParteAdversa = parteAdversa?.Trim(),
            Observacoes = observacoes?.Trim()
        };

        return Result<Processo>.Success(processo);
    }

    public Result AtualizarStatus(StatusProcesso novoStatus)
    {
        Status = novoStatus;
        UpdateTimestamp();
        return Result.Success();
    }

    public Result RegistrarMovimentacao(DateTime dataMovimentacao)
    {
        UltimaMovimentacao = dataMovimentacao;
        UpdateTimestamp();
        return Result.Success();
    }

    public Result DefinirProximoPrazo(DateTime prazo)
    {
        if (prazo <= DateTime.UtcNow)
            return Result.Failure(Error.Validation("Prazo deve ser uma data futura."));

        ProximoPrazo = prazo;
        UpdateTimestamp();
        return Result.Success();
    }

    public Result Atualizar(
        string titulo,
        FaseProcessual fase,
        string? tribunal,
        string? vara,
        string? juizResponsavel,
        string? parteAdversa,
        string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Length < 3)
            return Result.Failure(Error.Validation("Título deve ter pelo menos 3 caracteres."));

        Titulo = titulo.Trim();
        Fase = fase;
        Tribunal = tribunal?.Trim();
        Vara = vara?.Trim();
        JuizResponsavel = juizResponsavel?.Trim();
        ParteAdversa = parteAdversa?.Trim();
        Observacoes = observacoes?.Trim();
        UpdateTimestamp();

        return Result.Success();
    }
}
