namespace JurisAI.Domain.Entities;

using JurisAI.Domain.Common;

/// <summary>
/// Representa um honorário advocatício vinculado a um processo ou cliente.
/// </summary>
public class Honorario : Entity
{
    public string UserId { get; private set; }
    public string? ProcessoId { get; private set; }
    public string ClienteId { get; private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime DataVencimento { get; private set; }
    public DateTime? DataPagamento { get; private set; }
    public bool Pago { get; private set; }
    public string? FormaPagamento { get; private set; }
    public string? Observacoes { get; private set; }

    private Honorario()
    {
        UserId = null!; ClienteId = null!; Descricao = null!;
    }

    public static Result<Honorario> Registrar(
        string userId,
        string clienteId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        string? processoId = null,
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<Honorario>.Failure(Error.Validation("UserId é obrigatório."));

        if (string.IsNullOrWhiteSpace(clienteId))
            return Result<Honorario>.Failure(Error.Validation("ClienteId é obrigatório."));

        if (string.IsNullOrWhiteSpace(descricao))
            return Result<Honorario>.Failure(Error.Validation("Descrição é obrigatória."));

        if (valor <= 0)
            return Result<Honorario>.Failure(Error.Validation("Valor deve ser maior que zero."));

        var honorario = new Honorario
        {
            UserId = userId,
            ClienteId = clienteId,
            ProcessoId = processoId,
            Descricao = descricao.Trim(),
            Valor = valor,
            DataVencimento = dataVencimento,
            Pago = false,
            Observacoes = observacoes?.Trim()
        };

        return Result<Honorario>.Success(honorario);
    }

    public Result MarcarComoPago(string formaPagamento)
    {
        if (Pago)
            return Result.Failure(Error.Conflict("Honorário já foi marcado como pago."));

        if (string.IsNullOrWhiteSpace(formaPagamento))
            return Result.Failure(Error.Validation("Forma de pagamento é obrigatória."));

        Pago = true;
        DataPagamento = DateTime.UtcNow;
        FormaPagamento = formaPagamento.Trim();
        UpdateTimestamp();

        return Result.Success();
    }
}
