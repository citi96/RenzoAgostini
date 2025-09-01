namespace RenzoAgostini.Shared.Common
{
    public class Result<T>
    {
        public T? Value { get; private set; }
        public string? Error { get; private set; }
        public bool IsSuccess => Error == null;
        public bool IsFailure => !IsSuccess;

        private Result(T value)
        {
            Value = value;
        }

        private Result(string error)
        {
            Error = error;
        }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(string error) => new(error);

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
        }
    }
}