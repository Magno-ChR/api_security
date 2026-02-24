using Microsoft.AspNetCore.Mvc;
using api_security.domain.Results;

namespace api_security.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<TValue>(this Result<TValue> result)
    {
        // 1. Si es exitoso, devuelve 200 OK con el valor (y solo el valor)
        if (result.IsSuccess)
        {
            // Usamos result.Value porque ya sabemos que es exitoso.
            return new OkObjectResult(result);
        }

        // 2. Si es un fallo, devolvemos un DTO que no accede a Value (evita InvalidOperationException al serializar)
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

            ErrorType.Unauthorized => new ObjectResult(body)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            },

            ErrorType.Conflict => new ObjectResult(body)
            {
                StatusCode = StatusCodes.Status409Conflict
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
