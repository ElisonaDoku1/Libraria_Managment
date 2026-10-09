using System.Net;
using Library.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagmentSys.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                ConflictException => (HttpStatusCode.Conflict, ex.Message),
                ValidationAppException => (HttpStatusCode.BadRequest, ex.Message),
                AuthenticationFailedException => (HttpStatusCode.Unauthorized, ex.Message),
                DbUpdateException => (HttpStatusCode.Conflict, "The operation conflicts with related data."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred."),
            };

            if (status == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception");

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)status;
            await context.Response.WriteAsJsonAsync(new
            {
                status = (int)status,
                title,
            });
        }


    }
}
