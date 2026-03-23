namespace JurisAI.Domain.Tests.ValueObjects;

using FluentAssertions;
using JurisAI.Domain.ValueObjects;
using Xunit;

public class NumeroCNJTests
{
    [Theory]
    [InlineData("1234567-89.2024.8.26.0001")]
    [InlineData("0000001-01.2023.5.00.0001")]
    public void Create_ComNumeroValido_DeveRetornarSucesso(string numero)
    {
        var result = NumeroCNJ.Create(numero);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be(numero);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("1234567-89.2024.8.26")]
    [InlineData("abc-def.ghij.k.lm.nopq")]
    public void Create_ComNumeroInvalido_DeveRetornarFalha(string numero)
    {
        var result = NumeroCNJ.Create(numero);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
}
