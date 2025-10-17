namespace SteamApi.Application.Common
{
    public class Result
    {
        public bool Success { get; }
        public string? Error { get; }

        protected Result(bool success, string? error)
        {
            Success = success;
            Error = error;
        }

        public static Result Ok() => new Result(true, null);
        public static Result Fail(string error) => new Result(false, error);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(bool success, T? value, string? error) : base(success, error)
        {
            Value = value;
        }

        public static Result<T> Ok(T value) => new Result<T>(true, value, null);
        public static new Result<T> Fail(string error) => new Result<T>(false, default, error);
    }
}


