namespace JurisAI.Domain.Tests.Entities;

using FluentAssertions;
using JurisAI.Domain.Entities;
using Xunit;

public class HonorarioTests
{
    [Fact]
    public void Registrar_ComDadosValidos_DeveRetornarSucesso()
    {
        var result = Honorario.Registrar(
            "user-1", "cliente-1", "Honorário inicial",
            5000m, DateTime.UtcNow.AddDays(30));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Pago.Should().BeFalse();
    }

    [Fact]
    public void Registrar_ComValorZero_DeveRetornarFalha()
    {
        var result = Honorario.Registrar(
            "user-1", "cliente-1", "Honorário",
            0m, DateTime.UtcNow.AddDays(30));

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void MarcarComoPago_ComHonorarioPendente_DeveRetornarSucesso()
    {
        var honorario = Honorario.Registrar(
            "user-1", "cliente-1", "Honorário",
            5000m, DateTime.UtcNow.AddDays(30)).Value!;

        var result = honorario.MarcarComoPago("PIX");

        result.IsSuccess.Should().BeTrue();
        honorario.Pago.Should().BeTrue();
        honorario.DataPagamento.Should().NotBeNull();
        honorario.FormaPagamento.Should().Be("PIX");
    }

    [Fact]
    public void MarcarComoPago_ComHonorarioJaPago_DeveRetornarFalha()
    {
        var honorario = Honorario.Registrar(
            "user-1", "cliente-1", "Honorário",
            5000m, DateTime.UtcNow.AddDays(30)).Value!;
        honorario.MarcarComoPago("PIX");

        var result = honorario.MarcarComoPago("Cartão");

        result.IsSuccess.Should().BeFalse();
    }
}
