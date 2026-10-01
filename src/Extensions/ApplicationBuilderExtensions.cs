using WineFilesApi.Middleware;

namespace WineFilesApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseFoxProApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("DefaultCors");

        app.MapHealthChecks("/health");
        app.MapControllers();

        return app;
    }
}