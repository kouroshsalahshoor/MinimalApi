using FluentValidation;
using Microsoft.Identity.Web.Resource;

namespace MinimalApi.Endpoints;

public static class DemoEndpoints
{
    public static void ConfigureDemoEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Ok(">>> get"));
        app.MapGet("/{id:int}", (int id) =>
        {
            return Results.Ok($">>> id: {id}");
        });
        app.MapPost("/", () => ">>> post");


        var scopeRequiredByApi = app.Configuration["AzureAd:Scopes"] ?? "";
        var summaries = new[]
        {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

        app.MapGet("/weatherforecast", (HttpContext httpContext) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .WithOpenApi()
        .RequireAuthorization();
    }

    internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
