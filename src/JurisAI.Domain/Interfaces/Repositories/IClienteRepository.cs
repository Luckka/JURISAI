namespace JurisAI.Domain.Interfaces.Repositories;

using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;

public interface IClienteRepository
{
    Task<Result<Cliente>> GetByIdAsync(string userId, string clienteId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Cliente>>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<Result<Cliente>> CreateAsync(Cliente cliente, CancellationToken ct = default);
    Task<Result<Cliente>> UpdateAsync(Cliente cliente, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userId, string clienteId, CancellationToken ct = default);
}
