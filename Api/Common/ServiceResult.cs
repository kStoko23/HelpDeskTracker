namespace Api.Common;

public class ServiceResult
{
    public bool IsSuccess { get; protected init; }

    public ServiceErrorCode? ErrorCode { get; protected init; }
    public string? ErrorMessage { get; protected init; }

    protected ServiceResult()
    {
    }

    public static ServiceResult Success()
    {
        return new ServiceResult { IsSuccess = true };
    }

    public static ServiceResult Failure(ServiceErrorCode errorCode, string errorMessage)
    {
        return new ServiceResult { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
    }

    public static ServiceResult NotFound(string errorMessage) => Failure(ServiceErrorCode.NotFound, errorMessage);

    public static ServiceResult Unauthorized(string errorMessage) =>
        Failure(ServiceErrorCode.Unauthorized, errorMessage);

    public static ServiceResult Forbidden(string errorMessage) => Failure(ServiceErrorCode.Forbidden, errorMessage);

    public static ServiceResult Conflict(string errorMessage) => Failure(ServiceErrorCode.Conflict, errorMessage);

    public static ServiceResult BadRequest(string errorMessage) => Failure(ServiceErrorCode.BadRequest, errorMessage);

    public static ServiceResult ValidationError(string errorMessage) =>
        Failure(ServiceErrorCode.ValidationError, errorMessage);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private init; }

    private ServiceResult()
    {
    }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T> { IsSuccess = true, Data = data };
    }

    public static new ServiceResult<T> Failure(ServiceErrorCode errorCode, string errorMessage)
    {
        return new ServiceResult<T> { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
    }

    public static new ServiceResult<T> NotFound(string errorMessage) =>
        Failure(ServiceErrorCode.NotFound, errorMessage);

    public static new ServiceResult<T> Unauthorized(string errorMessage) =>
        Failure(ServiceErrorCode.Unauthorized, errorMessage);

    public static new ServiceResult<T> Forbidden(string errorMessage) =>
        Failure(ServiceErrorCode.Forbidden, errorMessage);

    public static new ServiceResult<T> Conflict(string errorMessage) =>
        Failure(ServiceErrorCode.Conflict, errorMessage);

    public static new ServiceResult<T> BadRequest(string errorMessage) =>
        Failure(ServiceErrorCode.BadRequest, errorMessage);

    public static new ServiceResult<T> ValidationError(string errorMessage) =>
        Failure(ServiceErrorCode.ValidationError, errorMessage);
}