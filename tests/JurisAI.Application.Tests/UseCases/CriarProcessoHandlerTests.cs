namespace JurisAI.Application.Tests.UseCases;

using FluentAssertions;
using JurisAI.Application.UseCases.Processos.CriarProcesso;
using JurisAI.Domain.Common;
using JurisAI.Domain.Entities;
using JurisAI.Domain.Enums;
using JurisAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

public class CriarProcessoHandlerTests
{
    private readonly IProcessoRepository _processoRepo = Substitute.For<IProcessoRepository>();
    private readonly IClienteRepository _clienteRepo = Substitute.For<IClienteRepository>();
    private readonly CriarProcessoHandler _handler;

    public CriarProcessoHandlerTests()
    {
        _handler = new CriarProcessoHandler(
            _processoRepo, _clienteRepo,
            NullLogger<CriarProcessoHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ComClienteExistente_DeveCriarProcesso()
    {
        // Arrange
        var cliente = Cliente.Criar(
            "user-1", "João Silva", "529.982.247-25",
            "joao@email.com").Value!;

        _clienteRepo.GetByIdAsync("user-1", "cliente-1", default)
            .Returns(Result<Cliente>.Success(cliente));

        var processoSaved = Processo.Criar(
            "user-1", "1234567-89.2024.8.26.0001", "cliente-1",
            "Ação de Cobrança", TipoAcao.Civil, FaseProcessual.Conhecimento).Value!;

        _processoRepo.CreateAsync(Arg.Any<Processo>(), default)
            .Returns(Result<Processo>.Success(processoSaved));

        var command = new CriarProcessoCommand(
            "user-1", "1234567-89.2024.8.26.0001", "cliente-1",
            "Ação de Cobrança", TipoAcao.Civil, FaseProcessual.Conhecimento);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.NumeroCNJ.Should().Be("1234567-89.2024.8.26.0001");
        result.Value.ClienteNome.Should().Be("João Silva");
    }

    [Fact]
    public async Task Handle_ComClienteInexistente_DeveRetornarFalha()
    {
        // Arrange
        _clienteRepo.GetByIdAsync("user-1", "cliente-nao-existe", default)
            .Returns(Result<Cliente>.Failure(Error.NotFound("Cliente")));

        var command = new CriarProcessoCommand(
            "user-1", "1234567-89.2024.8.26.0001", "cliente-nao-existe",
            "Ação de Cobrança", TipoAcao.Civil, FaseProcessual.Conhecimento);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Contain("NotFound");
    }
}
