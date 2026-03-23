namespace JurisAI.Domain.Entities;

using JurisAI.Domain.Common;

/// <summary>
/// Representa uma peça jurídica gerada pela IA ou manualmente.
/// </summary>
public class Peca : Entity
{
    public string UserId { get; private set; }
    public string? ProcessoId { get; private set; }
    public string Titulo { get; private set; }
    public string TipoPeca { get; private set; }
    public string Conteudo { get; private set; }
    public string? S3Key { get; private set; }
    public bool GeradaPorIA { get; private set; }
    public string? PromptUtilizado { get; private set; }
    public string? ModeloIA { get; private set; }
    public int? TokensUtilizados { get; private set; }

    private Peca()
    {
        UserId = null!; Titulo = null!; TipoPeca = null!; Conteudo = null!;
    }

    public static Result<Peca> Criar(
        string userId,
        string titulo,
        string tipoPeca,
        string conteudo,
        bool geradaPorIA = false,
        string? processoId = null,
        string? promptUtilizado = null,
        string? modeloIA = null,
        int? tokensUtilizados = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<Peca>.Failure(Error.Validation("UserId é obrigatório."));

        if (string.IsNullOrWhiteSpace(titulo))
            return Result<Peca>.Failure(Error.Validation("Título é obrigatório."));

        if (string.IsNullOrWhiteSpace(tipoPeca))
            return Result<Peca>.Failure(Error.Validation("Tipo de peça é obrigatório."));

        if (string.IsNullOrWhiteSpace(conteudo))
            return Result<Peca>.Failure(Error.Validation("Conteúdo é obrigatório."));

        var peca = new Peca
        {
            UserId = userId,
            ProcessoId = processoId,
            Titulo = titulo.Trim(),
            TipoPeca = tipoPeca.Trim(),
            Conteudo = conteudo,
            GeradaPorIA = geradaPorIA,
            PromptUtilizado = promptUtilizado,
            ModeloIA = modeloIA,
            TokensUtilizados = tokensUtilizados
        };

        return Result<Peca>.Success(peca);
    }

    public void DefinirS3Key(string key)
    {
        S3Key = key;
        UpdateTimestamp();
    }
}
