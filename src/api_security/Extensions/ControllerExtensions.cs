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

        // Si es un fallo, devolvemos un DTO que no accede a Value (evita InvalidOperationException al serializar)
        var error = result.Error;
        var body = ToFailureBody(result);

        return error.Type switch
        {
            ErrorType.NotFound => new ObjectResult(body)
            {
                StatusCode = StatusCodes.Status404NotFound
            },
            ErrorType.Validation => new ObjectResult(body)
            {
                StatusCode = StatusCodes.Status400BadRequest
            },
            ErrorType.Conflict => new ObjectResult(body)
            {
                StatusCode = StatusCodes.Status409Conflict
            },
            ErrorType.Unauthorized => new ObjectResult(body)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            },

            _ => new ObjectResult(body)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            },
        };
    }

    /// <summary>Objeto con la misma forma que Result pero sin acceder a Value (para serializar errores).</summary>
    private static object ToFailureBody<TValue>(Result<TValue> result) => new
    {
        value = (object?)null,
        result.IsSuccess,
        result.IsFailure,
        error = result.Error
    };
}
