namespace JurisAI.Domain.Entities;

using JurisAI.Domain.Common;

/// <summary>
/// Representa um prazo processual vinculado a um processo.
/// </summary>
public class Prazo : Entity
{
    public string UserId { get; private set; }
    public string ProcessoId { get; private set; }
    public string Descricao { get; private set; }
    public DateTime DataPrazo { get; private set; }
    public bool Cumprido { get; private set; }
    public DateTime? DataCumprimento { get; private set; }
    public string? Observacoes { get; private set; }
    public bool AlertaEnviado { get; private set; }

    // Propriedades calculadas
    public int DiasRestantes => (int)(DataPrazo - DateTime.UtcNow).TotalDays;
    public bool Vencido => !Cumprido && DataPrazo < DateTime.UtcNow;
    public bool Urgente => !Cumprido && DiasRestantes <= 3;
    public bool Atencao => !Cumprido && DiasRestantes > 3 && DiasRestantes <= 15;

    private Prazo()
    {
        UserId = null!; ProcessoId = null!; Descricao = null!;
    }

    public static Result<Prazo> Criar(
        string userId,
        string processoId,
        string descricao,
        DateTime dataPrazo,
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<Prazo>.Failure(Error.Validation("UserId é obrigatório."));

        if (string.IsNullOrWhiteSpace(processoId))
            return Result<Prazo>.Failure(Error.Validation("ProcessoId é obrigatório."));

        if (string.IsNullOrWhiteSpace(descricao))
            return Result<Prazo>.Failure(Error.Validation("Descrição é obrigatória."));

        var prazo = new Prazo
        {
            UserId = userId,
            ProcessoId = processoId,
            Descricao = descricao.Trim(),
            DataPrazo = dataPrazo,
            Cumprido = false,
            Observacoes = observacoes?.Trim()
        };

        return Result<Prazo>.Success(prazo);
    }

    public Result Cumprir()
    {
        if (Cumprido)
            return Result.Failure(Error.Conflict("Prazo já foi cumprido."));

        Cumprido = true;
        DataCumprimento = DateTime.UtcNow;
        UpdateTimestamp();

        return Result.Success();
    }

    public void MarcarAlertaEnviado()
    {
        AlertaEnviado = true;
        UpdateTimestamp();
    }
}
