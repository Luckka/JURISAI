namespace JurisAI.Application.UseCases.Clientes.CriarCliente;

public record CriarClienteCommand(
    string UserId,
    string Nome,
    string Documento,
    string Email,
    string? Telefone = null,
    string? Endereco = null,
    string? Observacoes = null
);
