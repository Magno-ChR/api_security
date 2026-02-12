using Microsoft.AspNetCore.Http.HttpResults;
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

        // 2. Si es un fallo, mapea el Error a un código de estado HTTP
        var error = result.Error;

        return error.Type switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(
                new
                {
                    error.Code,
                    error.Description
                }
            ), // 404 Not Found

            ErrorType.Validation => new BadRequestObjectResult(
                new
                {
                    error.Code,
                    error.Description,
                    Details = error.Description // Puedes usar Description para el mensaje de ArgumentException
                }
            ), // 400 Bad Request (Ideal para errores de validación)

            ErrorType.Unauthorized => new UnauthorizedResult(), // 401 Unauthorized

            // Caso por defecto para errores internos o genéricos (ErrorType.Problem, etc.)
            _ => new ObjectResult(
                new
                {
                    error.Code,
                    error.Description
                }
            )
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
    }
}
