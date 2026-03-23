namespace JurisAI.Domain.Interfaces.Repositories;

using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;

public interface IProcessoRepository
{
    Task<Result<Processo>> GetByIdAsync(string userId, string processoId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Processo>>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<Result<Processo>> CreateAsync(Processo processo, CancellationToken ct = default);
    Task<Result<Processo>> UpdateAsync(Processo processo, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userId, string processoId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Processo>>> GetByClienteIdAsync(string userId, string clienteId, CancellationToken ct = default);
}
