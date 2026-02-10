using JiraLite.Share.Common;

public class Result
{
    public bool IsSuccess { get; init; }
    public Error Error { get; init; } = Error.None;
    public bool IsFailure => !IsSuccess;

    protected Result() { }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(Error error) => new() { IsSuccess = false, Error = error };

    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(Error error) => new(error);
}

public class Result<T> : Result
{
    public T? Value { get; init; }

    protected internal Result() { }

    protected internal Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = Error.None;
    }

    protected internal Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }
}