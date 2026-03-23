namespace JurisAI.Application.UseCases.IA.ListarPecas;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class ListarPecasHandler : BaseHandler<ListarPecasHandler>
{
    private readonly IPecaRepository _pecaRepository;

    public ListarPecasHandler(
        IPecaRepository pecaRepository,
        ILogger<ListarPecasHandler> logger) : base(logger)
    {
        _pecaRepository = pecaRepository;
    }

    public async Task<Result<IReadOnlyList<PecaDto>>> HandleAsync(ListarPecasQuery query, CancellationToken ct = default)
    {
        var result = string.IsNullOrEmpty(query.ProcessoId)
            ? await _pecaRepository.GetByUserIdAsync(query.UserId, ct)
            : await _pecaRepository.GetByProcessoIdAsync(query.UserId, query.ProcessoId!, ct);

        return result.Match(
            pecas => Result<IReadOnlyList<PecaDto>>.Success(
                pecas.Select(p => new PecaDto(
                    p.Id, p.ProcessoId, p.Titulo, p.TipoPeca, p.Conteudo,
                    p.S3Key, p.GeradaPorIA, p.ModeloIA, p.TokensUtilizados, p.CreatedAt)).ToList().AsReadOnly()),
            error => Result<IReadOnlyList<PecaDto>>.Failure(error));
    }
}
