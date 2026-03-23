namespace JurisAI.Application.UseCases.Processos.ListarProcessos;

public record ListarProcessosQuery(string UserId, int Page = 1, int PageSize = 20);
