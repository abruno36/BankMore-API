namespace BankMore.API.Common
{
    public class Result<T>
    {
        public bool Succeeded { get; }
        public T Data { get; }
        public string ErrorMessage { get; }

        private Result(bool succeeded, T data, string errorMessage)
        {
            Succeeded = succeeded;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T data) => new Result<T>(true, data, null);
        public static Result<T> Fail(string errorMessage) => new Result<T>(false, default, errorMessage);
    }
}