namespace JurisAI.Domain.ValueObjects;

using JurisAI.Domain.Common;
using System.Text.RegularExpressions;

/// <summary>
/// Value Object para endereço de e-mail com validação de formato.
/// </summary>
public sealed record Email
{
    private static readonly Regex Pattern = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value.ToLowerInvariant();

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure(Error.Validation("E-mail não pode ser vazio."));

        var normalized = email.Trim();
        if (!Pattern.IsMatch(normalized))
            return Result<Email>.Failure(Error.Validation("E-mail inválido."));

        return Result<Email>.Success(new Email(normalized));
    }

    public override string ToString() => Value;
}
