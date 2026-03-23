namespace JurisAI.Domain.Tests.ValueObjects;

using FluentAssertions;
using JurisAI.Domain.ValueObjects;
using Xunit;

public class CpfCnpjTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Create_ComCpfValido_DeveRetornarSucesso(string cpf)
    {
        var result = CpfCnpj.Create(cpf);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCpf.Should().BeTrue();
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("000.000.000-00")]
    [InlineData("123.456.789-00")]
    public void Create_ComCpfInvalido_DeveRetornarFalha(string cpf)
    {
        var result = CpfCnpj.Create(cpf);

        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData("11.222.333/0001-81")]
    [InlineData("11222333000181")]
    public void Create_ComCnpjValido_DeveRetornarSucesso(string cnpj)
    {
        var result = CpfCnpj.Create(cnpj);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCnpj.Should().BeTrue();
    }
}
