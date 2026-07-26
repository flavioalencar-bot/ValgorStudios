using System.Net;
using Valgor.Application.Common.Results;

namespace Valgor.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return ToFailureResult(result.Error!);
    }

    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return ToFailureResult(result.Error!);
    }

    private static IResult ToFailureResult(Error error)
    {
        var status = error.Code switch
        {
            "validation" => HttpStatusCode.BadRequest,
            "unauthorized" => HttpStatusCode.Unauthorized,
            "not_found" => HttpStatusCode.NotFound,
            "conflict" => HttpStatusCode.Conflict,
            _ => HttpStatusCode.BadRequest
        };

        return Results.Json(
            new { code = error.Code, message = error.Message },
            statusCode: (int)status);
    }
}
