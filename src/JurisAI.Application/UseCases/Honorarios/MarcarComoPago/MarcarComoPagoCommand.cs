namespace JurisAI.Application.UseCases.Honorarios.MarcarComoPago;

public record MarcarComoPagoCommand(string UserId, string HonorarioId, string FormaPagamento);
