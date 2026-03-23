namespace JurisAI.Application.Common;

using Microsoft.Extensions.Logging;

public abstract class BaseHandler<THandler>
{
    protected readonly ILogger<THandler> Logger;

    protected BaseHandler(ILogger<THandler> logger)
    {
        Logger = logger;
    }
}
