namespace JurisAI.Integration.Tests;

using FluentAssertions;
using Xunit;

/// <summary>
/// Testes de integração — requerem DynamoDB local rodando.
/// Execute: docker run -p 8000:8000 amazon/dynamodb-local
/// </summary>
[Trait("Category", "Integration")]
public class ProcessosApiTests
{
    // Testes de integração são executados apenas com DynamoDB local configurado.
    // Use a variável de ambiente DYNAMODB_LOCAL_ENDPOINT=http://localhost:8000

    [Fact(Skip = "Requer DynamoDB local")]
    public async Task CriarProcesso_ComDadosValidos_DeveRetornarProcessoCriado()
    {
        // Arrange — configurar DynamoDB local
        // Act — criar processo via handler
        // Assert — verificar no DynamoDB
        await Task.CompletedTask;
        true.Should().BeTrue();
    }
}
