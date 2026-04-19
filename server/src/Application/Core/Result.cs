namespace Application.Core;
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? Error { get; set; }
    public int Code { get; set; }

    public static Result<T> Success(T value) =>
        new() { IsSuccess = true, Value = value, Code = 200 };
    public static Result<T> Failure(string error, int code) => new Result<T>()
    {
        IsSuccess = false,
        Error = error,
        Code = code
    };

    public static Result<T> Failure(Error error) => new Result<T>()
    {
        IsSuccess = false,
        Error = error.Message,
        Code = error.Code
    };

    public static Result<T> Custom(T? value, string? error, int code, bool success) =>
        new Result<T>()
        {
            Value = value,
            Error = error,
            Code = code,
            IsSuccess = success
        };
    
    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>((int code, string error) failure) =>
        Failure(failure.error, failure.code);
}
