namespace JurisAI.Domain.Common;

/// <summary>
/// Representa o resultado de uma operação que pode ter sucesso ou falha.
/// Evita o uso de exceptions para controle de fluxo.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public Error? Error { get; private set; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(Error error) { IsSuccess = false; Error = error; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}

/// <summary>
/// Resultado sem valor de retorno (operações void).
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public Error? Error { get; private set; }

    private Result() { IsSuccess = true; }
    private Result(Error error) { IsSuccess = false; Error = error; }

    public static Result Success() => new();
    public static Result Failure(Error error) => new(error);
    public static implicit operator Result(Error error) => new(error);

    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error!);
}
