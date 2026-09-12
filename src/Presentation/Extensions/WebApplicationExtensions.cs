namespace CleanMinimalApi.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
using CleanMinimalApi.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Serilog;

[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        #region Logging

        app.UseExceptionHandler();
        app.UseHttpLogging();
        app.UseSerilogRequestLogging();

        #endregion Logging

        #region Security

        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("local"))
        {
            app.UseHsts();
        }

        #endregion Security

        #region API Configuration

        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("local"))
        {
            app.UseHttpsRedirection();
        }

        #endregion API Configuration

        #region Swagger

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("local"))
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "CleanMinimalApi API v1"));
        }

        #endregion Swagger

        #region MinimalApi

        app.MapVersionEndpoints();
        app.MapReadingEndpoints();
        app.MapEvidenceEndpoints();
        app.MapReportEndpoints();

        #endregion MinimalApi

        return app;
    }
}
