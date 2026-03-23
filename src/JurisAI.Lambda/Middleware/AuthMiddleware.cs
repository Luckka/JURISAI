namespace JurisAI.Lambda.Middleware;

using Amazon.Lambda.APIGatewayEvents;

/// <summary>
/// Extrai o userId do token JWT validado pelo Cognito Authorizer do API Gateway.
/// </summary>
public static class AuthMiddleware
{
    public static string? GetUserId(APIGatewayHttpApiV2ProxyRequest request)
    {
        // O Cognito Authorizer injeta o sub (userId) nos claims do JWT
        if (request.RequestContext?.Authorizer?.Jwt?.Claims != null)
        {
            if (request.RequestContext.Authorizer.Jwt.Claims.TryGetValue("sub", out var sub))
                return sub;
        }

        // Fallback para desenvolvimento local
        if (request.Headers != null &&
            request.Headers.TryGetValue("x-user-id", out var userId))
            return userId;

        return null;
    }
}
