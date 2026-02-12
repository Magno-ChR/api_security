using Microsoft.AspNetCore.Mvc;
using api_security.domain.Results;

namespace api_security.Extensions;

public static class ControllerExtensions
{
    public static IActionResult HandleResult<TValue>(this ControllerBase controller, Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            // Devuelve solo el valor serializado dentro de un 200 OK
            return controller.Ok(result);
        }

        // Si es un fallo, usa tu lógica de mapeo de errores
        var error = result.Error;

        return error.Type switch
        {
            ErrorType.NotFound => controller.NotFound(Error(result)), // 404
            ErrorType.Validation => controller.BadRequest(Error(result)), // 400
            ErrorType.Conflict => controller.Conflict(new { error.Code, error.Description }), // 409
            ErrorType.Unauthorized => controller.Unauthorized(), // 401

            _ => controller.Problem(error.Description), // 500
        };
    }

    private static Result Error<TValue>(Result<TValue> result)
    {
        return Result.Failure(result.Error);
    }
}
