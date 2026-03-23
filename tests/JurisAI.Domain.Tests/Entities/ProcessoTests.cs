namespace JurisAI.Domain.Tests.Entities;

using FluentAssertions;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Enums;
using Xunit;

public class ProcessoTests
{
    private const string ValidUserId = "user-123";
    private const string ValidNumeroCNJ = "1234567-89.2024.8.26.0001";
    private const string ValidClienteId = "cliente-123";
    private const string ValidTitulo = "Ação de Cobrança";

    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarSucesso()
    {
        var result = Processo.Criar(
            ValidUserId, ValidNumeroCNJ, ValidClienteId,
            ValidTitulo, TipoAcao.Civil, FaseProcessual.Conhecimento);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(StatusProcesso.Ativo);
        result.Value.UserId.Should().Be(ValidUserId);
    }

    [Fact]
    public void Criar_ComNumeroCNJInvalido_DeveRetornarFalha()
    {
        var result = Processo.Criar(
            ValidUserId, "numero-invalido", ValidClienteId,
            ValidTitulo, TipoAcao.Civil, FaseProcessual.Conhecimento);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Contain("Validation");
    }

    [Fact]
    public void Criar_ComTituloVazio_DeveRetornarFalha()
    {
        var result = Processo.Criar(
            ValidUserId, ValidNumeroCNJ, ValidClienteId,
            "", TipoAcao.Civil, FaseProcessual.Conhecimento);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void DefinirProximoPrazo_ComDataPassada_DeveRetornarFalha()
    {
        var processoResult = Processo.Criar(
            ValidUserId, ValidNumeroCNJ, ValidClienteId,
            ValidTitulo, TipoAcao.Civil, FaseProcessual.Conhecimento);

        var result = processoResult.Value!.DefinirProximoPrazo(DateTime.UtcNow.AddDays(-1));

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AtualizarStatus_DeveAtualizarStatusETimestamp()
    {
        var processo = Processo.Criar(
            ValidUserId, ValidNumeroCNJ, ValidClienteId,
            ValidTitulo, TipoAcao.Civil, FaseProcessual.Conhecimento).Value!;

        var antes = processo.UpdatedAt;
        System.Threading.Thread.Sleep(10);
        processo.AtualizarStatus(StatusProcesso.Arquivado);

        processo.Status.Should().Be(StatusProcesso.Arquivado);
        processo.UpdatedAt.Should().BeAfter(antes);
    }
}
