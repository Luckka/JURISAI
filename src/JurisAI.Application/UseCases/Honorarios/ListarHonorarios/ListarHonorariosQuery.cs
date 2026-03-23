namespace JurisAI.Application.UseCases.Honorarios.ListarHonorarios;

public record ListarHonorariosQuery(string UserId, bool? ApenasPendentes = null);
