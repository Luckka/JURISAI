namespace JurisAI.Domain.ValueObjects;

using JurisAI.Domain.Common;

/// <summary>
/// Value Object para CPF ou CNPJ com validação de dígitos verificadores.
/// </summary>
public sealed record CpfCnpj
{
    public string Value { get; }
    public bool IsCpf => Value.Length == 11;
    public bool IsCnpj => Value.Length == 14;

    private CpfCnpj(string value) => Value = value;

    public static Result<CpfCnpj> Create(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return Result<CpfCnpj>.Failure(Error.Validation("CPF/CNPJ não pode ser vazio."));

        // Remove formatação
        var digits = new string(documento.Where(char.IsDigit).ToArray());

        if (digits.Length == 11)
        {
            if (!ValidateCpf(digits))
                return Result<CpfCnpj>.Failure(Error.Validation("CPF inválido."));
            return Result<CpfCnpj>.Success(new CpfCnpj(digits));
        }

        if (digits.Length == 14)
        {
            if (!ValidateCnpj(digits))
                return Result<CpfCnpj>.Failure(Error.Validation("CNPJ inválido."));
            return Result<CpfCnpj>.Success(new CpfCnpj(digits));
        }

        return Result<CpfCnpj>.Failure(Error.Validation("CPF deve ter 11 dígitos ou CNPJ 14 dígitos."));
    }

    private static bool ValidateCpf(string cpf)
    {
        // Verifica se todos os dígitos são iguais
        if (cpf.Distinct().Count() == 1) return false;

        // Valida primeiro dígito verificador
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += int.Parse(cpf[i].ToString()) * (10 - i);
        int remainder = (sum * 10) % 11;
        if (remainder == 10 || remainder == 11) remainder = 0;
        if (remainder != int.Parse(cpf[9].ToString())) return false;

        // Valida segundo dígito verificador
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += int.Parse(cpf[i].ToString()) * (11 - i);
        remainder = (sum * 10) % 11;
        if (remainder == 10 || remainder == 11) remainder = 0;
        return remainder == int.Parse(cpf[10].ToString());
    }

    private static bool ValidateCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false;

        int[] multipliers1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multipliers2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        int sum = cnpj.Take(12).Select((c, i) => int.Parse(c.ToString()) * multipliers1[i]).Sum();
        int remainder = sum % 11;
        int d1 = remainder < 2 ? 0 : 11 - remainder;
        if (d1 != int.Parse(cnpj[12].ToString())) return false;

        sum = cnpj.Take(13).Select((c, i) => int.Parse(c.ToString()) * multipliers2[i]).Sum();
        remainder = sum % 11;
        int d2 = remainder < 2 ? 0 : 11 - remainder;
        return d2 == int.Parse(cnpj[13].ToString());
    }

    public string Formatted => IsCpf
        ? $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}"
        : $"{Value[..2]}.{Value[2..5]}.{Value[5..8]}/{Value[8..12]}-{Value[12..]}";

    public override string ToString() => Formatted;
}
