using Microsoft.AspNetCore.Mvc;

namespace Api.Common;

public static class ServiceResultHelper
{
    public static IActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
    {
        return result.ErrorCode switch
        {
            ServiceErrorCode.BadRequest => controller.BadRequest(result.ErrorMessage),
            ServiceErrorCode.ValidationError => controller.UnprocessableEntity(result.ErrorMessage),
            ServiceErrorCode.Unauthorized => controller.Unauthorized(result.ErrorMessage),
            ServiceErrorCode.Forbidden => controller.Forbid(),
            ServiceErrorCode.NotFound => controller.NotFound(result.ErrorMessage),
            ServiceErrorCode.Conflict => controller.Conflict(result.ErrorMessage),
            _ => controller.StatusCode(500, result.ErrorMessage)
        };
    }
}