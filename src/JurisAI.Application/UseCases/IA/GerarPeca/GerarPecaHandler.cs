namespace JurisAI.Application.UseCases.IA.GerarPeca;

using JurisAI.Application.Common;
using JurisAI.Application.DTOs;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Interfaces.Repositories;
using JurisAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

public class GerarPecaHandler : BaseHandler<GerarPecaHandler>
{
    private readonly IIAService _iaService;
    private readonly IPecaRepository _pecaRepository;
    private readonly IProcessoRepository _processoRepository;

    public GerarPecaHandler(
        IIAService iaService,
        IPecaRepository pecaRepository,
        IProcessoRepository processoRepository,
        ILogger<GerarPecaHandler> logger) : base(logger)
    {
        _iaService = iaService;
        _pecaRepository = pecaRepository;
        _processoRepository = processoRepository;
    }

    public async Task<Result<PecaDto>> HandleAsync(GerarPecaCommand command, CancellationToken ct = default)
    {
        string? processoNumero = null;
        string? parteAdversa = null;

        // Busca dados do processo se fornecido
        if (!string.IsNullOrEmpty(command.ProcessoId))
        {
            var processoResult = await _processoRepository.GetByIdAsync(command.UserId, command.ProcessoId, ct);
            if (processoResult.IsSuccess)
            {
                processoNumero = processoResult.Value!.NumeroCNJ.Value;
                parteAdversa = processoResult.Value.ParteAdversa;
            }
        }

        var request = new GerarPecaRequest(command.TipoPeca, command.Contexto, processoNumero, parteAdversa);
        var iaResult = await _iaService.GerarPecaAsync(request, ct);

        if (!iaResult.IsSuccess)
            return Result<PecaDto>.Failure(iaResult.Error!);

        var iaResponse = iaResult.Value!;

        var pecaResult = Peca.Criar(
            command.UserId,
            $"{command.TipoPeca} - {DateTime.UtcNow:dd/MM/yyyy}",
            command.TipoPeca,
            iaResponse.Conteudo,
            geradaPorIA: true,
            processoId: command.ProcessoId,
            promptUtilizado: command.Contexto,
            modeloIA: iaResponse.ModeloUtilizado,
            tokensUtilizados: iaResponse.TokensUsados);

        if (!pecaResult.IsSuccess)
            return Result<PecaDto>.Failure(pecaResult.Error!);

        var saveResult = await _pecaRepository.CreateAsync(pecaResult.Value!, ct);
        if (!saveResult.IsSuccess)
            return Result<PecaDto>.Failure(saveResult.Error!);

        var p = saveResult.Value!;
        Logger.LogInformation("Peça {PecaId} gerada para usuário {UserId} com {Tokens} tokens",
            p.Id, command.UserId, iaResponse.TokensUsados);

        return Result<PecaDto>.Success(new PecaDto(
            p.Id, p.ProcessoId, p.Titulo, p.TipoPeca, p.Conteudo,
            p.S3Key, p.GeradaPorIA, p.ModeloIA, p.TokensUtilizados, p.CreatedAt));
    }
}
