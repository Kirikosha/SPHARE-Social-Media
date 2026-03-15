namespace API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unhandled exception occured. Path: {Path} TraceId: {TraceId}", context.Request.Path, context.TraceIdentifier);

            context.Response.StatusCode = 500;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Internal server error"
            });
        }
    }
}