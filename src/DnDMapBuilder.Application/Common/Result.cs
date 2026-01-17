namespace DnDMapBuilder.Application.Common;

/// <summary>
/// Represents the result of an operation that returns no data, indicating success or failure.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the list of errors if operation failed.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    private Result(bool isSuccess, string? error, IReadOnlyList<string>? errors)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? new List<string>();
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful Result</returns>
    public static Result Success()
    {
        return new Result(true, null, null);
    }

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message</param>
    /// <returns>A failed Result</returns>
    public static Result Failure(string error)
    {
        return new Result(false, error, new[] { error });
    }

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    /// <param name="errors">The list of error messages</param>
    /// <returns>A failed Result</returns>
    public static Result Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new Result(false, errorList.FirstOrDefault(), errorList);
    }
}

/// <summary>
/// Represents the result of an operation that returns data, indicating success or failure.
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the data returned by the operation (null if failed).
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Gets the error message if operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the list of errors if operation failed.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    private Result(bool isSuccess, T? data, string? error, IReadOnlyList<string>? errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Errors = errors ?? new List<string>();
    }

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    /// <param name="data">The data to return</param>
    /// <returns>A successful Result<T></returns>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, null, null);
    }

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message</param>
    /// <returns>A failed Result<T></returns>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error, new[] { error });
    }

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    /// <param name="errors">The list of error messages</param>
    /// <returns>A failed Result<T></returns>
    public static Result<T> Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new Result<T>(false, default, errorList.FirstOrDefault(), errorList);
    }
}
