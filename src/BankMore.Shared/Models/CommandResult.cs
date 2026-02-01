namespace BankMore.Shared.Models
{
    public class CommandResult<T>
    {
        public bool Success { get; }
        public T? Data { get; }
        public string? ErrorMessage { get; }
        public string? ErrorType { get; }

        private CommandResult(bool success, T? data, string? errorMessage, string? errorType)
        {
            Success = success;
            Data = data;
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public static CommandResult<T> SuccessResult(T data) => new(true, data, null, null);

        public static CommandResult<T> FailureResult(string errorMessage, string errorType)
            => new(false, default, errorMessage, errorType);
    }
}