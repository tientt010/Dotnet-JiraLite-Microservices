using JiraLite.Share.Common;

// 2. Class Result gốc (Dành cho các action KHÔNG trả về data - tương đương void/Task)
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public Error[]? ValidationErrors { get; }

    protected Result(bool isSuccess, Error error, Error[]? validationErrors = null)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException();
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result ValidationFailure(Error[] errors) => new(false, new Error("ValidationError", "Dữ liệu đầu vào không hợp lệ"), errors);
}

// 3. Class Result<TValue> kế thừa từ Result (Dành cho các action CÓ trả về data)
public class Result<TValue> : Result
{
    public TValue? Value { get; }

    protected Result(bool isSuccess, Error error, TValue? value, Error[]? validationErrors = null)
        : base(isSuccess, error, validationErrors)
    {
        Value = value;
    }

    public static Result<TValue> Success(TValue value) => new(true, Error.None, value);
    public static new Result<TValue> Failure(Error error) => new(false, error, default);
    public static new Result<TValue> ValidationFailure(Error[] errors) => new(false, new Error("ValidationError", "Dữ liệu đầu vào không hợp lệ"), default, errors);
}