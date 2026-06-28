namespace Api.Common;

public enum ServiceErrorCode
{
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    ValidationError = 422,
}