namespace JurisAI.Domain.Entities;

using JurisAI.Domain.Common;
using JurisAI.Domain.ValueObjects;

/// <summary>
/// Entidade que representa um cliente do escritório de advocacia.
/// </summary>
public class Cliente : Entity
{
    public string UserId { get; private set; }
    public string Nome { get; private set; }
    public CpfCnpj Documento { get; private set; }
    public Email Email { get; private set; }
    public string? Telefone { get; private set; }
    public string? Endereco { get; private set; }
    public string? Observacoes { get; private set; }
    public bool Ativo { get; private set; } = true;

    private Cliente() { UserId = null!; Nome = null!; Documento = null!; Email = null!; }

    public static Result<Cliente> Criar(
        string userId,
        string nome,
        string documento,
        string email,
        string? telefone = null,
        string? endereco = null,
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<Cliente>.Failure(Error.Validation("UserId é obrigatório."));

        if (string.IsNullOrWhiteSpace(nome) || nome.Length < 2)
            return Result<Cliente>.Failure(Error.Validation("Nome deve ter pelo menos 2 caracteres."));

        var documentoResult = CpfCnpj.Create(documento);
        if (!documentoResult.IsSuccess)
            return Result<Cliente>.Failure(documentoResult.Error!);

        var emailResult = Email.Create(email);
        if (!emailResult.IsSuccess)
            return Result<Cliente>.Failure(emailResult.Error!);

        var cliente = new Cliente
        {
            UserId = userId,
            Nome = nome.Trim(),
            Documento = documentoResult.Value!,
            Email = emailResult.Value!,
            Telefone = telefone?.Trim(),
            Endereco = endereco?.Trim(),
            Observacoes = observacoes?.Trim()
        };

        return Result<Cliente>.Success(cliente);
    }

    public Result Atualizar(string nome, string? telefone, string? endereco, string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Length < 2)
            return Result.Failure(Error.Validation("Nome deve ter pelo menos 2 caracteres."));

        Nome = nome.Trim();
        Telefone = telefone?.Trim();
        Endereco = endereco?.Trim();
        Observacoes = observacoes?.Trim();
        UpdateTimestamp();

        return Result.Success();
    }

    public void Desativar()
    {
        Ativo = false;
        UpdateTimestamp();
    }
}
