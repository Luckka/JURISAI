namespace JurisAI.Infrastructure.Services;

using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using JurisAI.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Serviço de autenticação via Amazon Cognito.
/// Valida tokens JWT e gerencia usuários.
/// </summary>
public class CognitoAuthService
{
    private readonly IAmazonCognitoIdentityProvider _cognito;
    private readonly ILogger<CognitoAuthService> _logger;
    private readonly string _userPoolId;
    private readonly string _clientId;

    public CognitoAuthService(
        IAmazonCognitoIdentityProvider cognito,
        IConfiguration configuration,
        ILogger<CognitoAuthService> logger)
    {
        _cognito = cognito;
        _logger = logger;
        _userPoolId = configuration["COGNITO_USER_POOL_ID"]
            ?? throw new InvalidOperationException("COGNITO_USER_POOL_ID não configurado");
        _clientId = configuration["COGNITO_CLIENT_ID"]
            ?? throw new InvalidOperationException("COGNITO_CLIENT_ID não configurado");
    }

    public async Task<Result<string>> ObterEmailAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var request = new AdminGetUserRequest
            {
                UserPoolId = _userPoolId,
                Username = userId
            };

            var response = await _cognito.AdminGetUserAsync(request, ct);
            var email = response.UserAttributes
                .FirstOrDefault(a => a.Name == "email")?.Value;

            if (string.IsNullOrEmpty(email))
                return Result<string>.Failure(Error.NotFound("Email do usuário"));

            return Result<string>.Success(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter email do usuário {UserId}", userId);
            return Result<string>.Failure(Error.ExternalService("Cognito", ex.Message));
        }
    }
}
