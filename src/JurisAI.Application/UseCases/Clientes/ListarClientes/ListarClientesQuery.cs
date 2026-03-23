namespace JurisAI.Application.UseCases.Clientes.ListarClientes;

public record ListarClientesQuery(string UserId, int Page = 1, int PageSize = 20);
