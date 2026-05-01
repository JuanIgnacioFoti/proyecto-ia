using System.Net;
using System.Text.Json;
using EsportsApp.API.Middleware;
using EsportsApp.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace EsportsApp.Tests.Unit.Middleware;

/// <summary>
/// GlobalExceptionMiddleware HTTP error response mapping tests (FR-040, FR-041, FR-042, FR-043).
/// Verifies 400/403/404/409/422/500 status codes are returned for the correct exception types.
/// </summary>
[TestClass]
public class GlobalExceptionMiddlewareTests
{
    private static async Task<(int StatusCode, string Body)> InvokeMiddlewareWithException(Exception ex)
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        int? capturedStatus = null;
        var bodyStream = new MemoryStream();

        var context = new DefaultHttpContext();
        context.Response.Body = bodyStream;
        // Capture the status code assignment
        context.Response.StatusCode = 200;

        var middleware = new GlobalExceptionMiddleware(
            _ => throw ex,
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        capturedStatus = context.Response.StatusCode;
        bodyStream.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(bodyStream).ReadToEndAsync();
        return (capturedStatus!.Value, body);
    }

    [TestMethod]
    public async Task NotFoundException_Returns404()
    {
        var (status, _) = await InvokeMiddlewareWithException(new NotFoundException("Not found."));
        Assert.AreEqual(404, status);
    }

    [TestMethod]
    public async Task ConflictException_Returns409()
    {
        var (status, _) = await InvokeMiddlewareWithException(new ConflictException("Conflict."));
        Assert.AreEqual(409, status);
    }

    [TestMethod]
    public async Task ForbiddenException_Returns403()
    {
        var (status, _) = await InvokeMiddlewareWithException(new ForbiddenException("Forbidden."));
        Assert.AreEqual(403, status);
    }

    [TestMethod]
    public async Task UnauthorizedException_Returns401()
    {
        var (status, _) = await InvokeMiddlewareWithException(new UnauthorizedAccessException("Unauthorized."));
        Assert.AreEqual(401, status);
    }

    [TestMethod]
    public async Task ValidationException_Returns400()
    {
        var (status, _) = await InvokeMiddlewareWithException(
            new EsportsApp.Application.Exceptions.ValidationException("field", "Invalid."));
        Assert.AreEqual(400, status);
    }

    [TestMethod]
    public async Task UnprocessableEntityException_Returns422()
    {
        var (status, _) = await InvokeMiddlewareWithException(new UnprocessableEntityException("Unprocessable."));
        Assert.AreEqual(422, status);
    }

    // ── Unhandled exception returns 500 with generic body (FR-040) ─────────────

    [TestMethod]
    public async Task UnhandledException_Returns500()
    {
        var (status, _) = await InvokeMiddlewareWithException(new InvalidOperationException("boom"));
        Assert.AreEqual(500, status);
    }

    [TestMethod]
    public async Task UnhandledException_Returns500_WithGenericBody()
    {
        var (_, body) = await InvokeMiddlewareWithException(new InvalidOperationException("sensitive details here"));

        // The 500 body must NOT expose the raw exception message (generic body only)
        Assert.IsFalse(body.Contains("sensitive details here"), "500 body must not leak exception details.");
        Assert.IsTrue(body.Contains("unexpected") || body.Contains("internal_server_error"),
            "500 body should contain a generic error indicator.");
    }

    // ── Response body type fields ─────────────────────────────────────────────

    [TestMethod]
    public async Task NotFoundException_BodyContainsTypeField()
    {
        var (_, body) = await InvokeMiddlewareWithException(new NotFoundException("Not found."));
        var doc = JsonDocument.Parse(body);
        Assert.IsTrue(doc.RootElement.TryGetProperty("type", out var type));
        Assert.AreEqual("not_found", type.GetString());
    }

    [TestMethod]
    public async Task ValidationException_BodyContainsErrors()
    {
        var (_, body) = await InvokeMiddlewareWithException(
            new EsportsApp.Application.Exceptions.ValidationException("email", "Invalid email."));
        var doc = JsonDocument.Parse(body);
        Assert.IsTrue(doc.RootElement.TryGetProperty("errors", out _), "ValidationException body must contain 'errors' field.");
    }
}
