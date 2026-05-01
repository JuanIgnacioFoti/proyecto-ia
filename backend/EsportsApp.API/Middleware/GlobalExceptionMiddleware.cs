using System.Text.Json;
using EsportsApp.Application.Exceptions;

namespace EsportsApp.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/problem+json";

        int statusCode;
        object body;

        switch (ex)
        {
            case NotFoundException nfe:
                statusCode = 404; body = new { type = "not_found", message = nfe.Message }; break;
            case ConflictException ce:
                statusCode = 409; body = new { type = "conflict", message = ce.Message }; break;
            case ForbiddenException fe:
                statusCode = 403; body = new { type = "forbidden", message = fe.Message }; break;
            case UnauthorizedAccessException uae:
                statusCode = 401; body = new { type = "unauthorized", message = uae.Message }; break;
            case EsportsApp.Application.Exceptions.ValidationException ve:
                statusCode = 400; body = new { type = "validation_error", errors = ve.Errors }; break;
            case UnprocessableEntityException ue:
                statusCode = 422; body = new { type = "unprocessable_entity", message = ue.Message }; break;
            default:
                statusCode = 500; body = new { type = "internal_server_error", message = "An unexpected error occurred." }; break;
        }

        if (statusCode == 500)
        {
            _logger.LogError(ex, "Unhandled exception");
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        }));
    }
}
