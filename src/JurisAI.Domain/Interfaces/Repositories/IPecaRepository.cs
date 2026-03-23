namespace JurisAI.Domain.Interfaces.Repositories;

using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;

public interface IPecaRepository
{
    Task<Result<Peca>> GetByIdAsync(string userId, string pecaId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Peca>>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Result<Peca>> CreateAsync(Peca peca, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Peca>>> GetByProcessoIdAsync(string userId, string processoId, CancellationToken ct = default);
}
