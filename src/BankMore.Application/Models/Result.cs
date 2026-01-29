namespace BankMore.Application.Models;

public class Result<T> : Result
{
    public T Data { get; init; }

    protected internal Result(T data, bool success, string errorCode, string errorMessage)
        : base(success, errorCode, errorMessage)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(data, true, null, null);
    public static new Result<T> Failure(string errorCode, string errorMessage)
        => new(default, false, errorCode, errorMessage);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    protected Result(bool success, string errorCode, string errorMessage)
    {
        IsSuccess = success;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string errorCode, string errorMessage)
        => new(false, errorCode, errorMessage);
}