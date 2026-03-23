namespace JurisAI.Application.UseCases.Processos.CriarProcesso;

using FluentValidation;

public class CriarProcessoValidator : AbstractValidator<CriarProcessoCommand>
{
    public CriarProcessoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId é obrigatório.");
        RuleFor(x => x.NumeroCNJ).NotEmpty().WithMessage("Número CNJ é obrigatório.");
        RuleFor(x => x.ClienteId).NotEmpty().WithMessage("ClienteId é obrigatório.");
        RuleFor(x => x.Titulo).NotEmpty().MinimumLength(3).WithMessage("Título deve ter pelo menos 3 caracteres.");
    }
}
