namespace JurisAI.Domain.Interfaces.Repositories;

using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;

public interface IHonorarioRepository
{
    Task<Result<Honorario>> GetByIdAsync(string userId, string honorarioId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Honorario>>> GetByUserIdAsync(string userId, bool? apenasPendentes = null, CancellationToken ct = default);
    Task<Result<Honorario>> CreateAsync(Honorario honorario, CancellationToken ct = default);
    Task<Result<Honorario>> UpdateAsync(Honorario honorario, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Honorario>>> GetByProcessoIdAsync(string userId, string processoId, CancellationToken ct = default);
}
