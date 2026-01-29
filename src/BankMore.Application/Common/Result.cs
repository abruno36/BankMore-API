namespace BankMore.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    public Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    public Result(string errorCode, string errorMessage)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string errorCode, string errorMessage) => new(errorCode, errorMessage);
}