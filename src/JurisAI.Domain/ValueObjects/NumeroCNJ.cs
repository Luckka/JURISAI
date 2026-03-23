namespace JurisAI.Domain.ValueObjects;

using JurisAI.Domain.Common;
using System.Text.RegularExpressions;

/// <summary>
/// Número de processo no padrão CNJ: NNNNNNN-DD.AAAA.J.TT.OOOO
/// </summary>
public sealed record NumeroCNJ
{
    private static readonly Regex Pattern = new(
        @"^\d{7}-\d{2}\.\d{4}\.\d\.\d{2}\.\d{4}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private NumeroCNJ(string value) => Value = value;

    public static Result<NumeroCNJ> Create(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return Result<NumeroCNJ>.Failure(Error.Validation("Número CNJ não pode ser vazio."));

        var normalized = numero.Trim();
        if (!Pattern.IsMatch(normalized))
            return Result<NumeroCNJ>.Failure(
                Error.Validation("Número CNJ inválido. Use o formato: NNNNNNN-DD.AAAA.J.TT.OOOO"));

        return Result<NumeroCNJ>.Success(new NumeroCNJ(normalized));
    }

    public override string ToString() => Value;
}
