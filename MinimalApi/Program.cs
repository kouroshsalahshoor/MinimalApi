using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using MinimalApi.Data;
using MinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/categories", () => Results.Ok(CategoriesStore.Categories.ToList())).WithName("GetCategories")
        .Produces<List<Category>>(StatusCodes.Status200OK)
        ;
app.MapGet("/api/category/{id:int}", (int id) =>
{
    if (id < 1)
    {
        return Results.BadRequest("Invalid");
    }

    var model = CategoriesStore.Categories.FirstOrDefault(x => x.Id == id);
    if (model == null)
        return Results.NotFound();
    return Results.Ok(model);
})
    .WithName("GetCategory")
    .Produces<Category>(StatusCodes.Status200OK)
    .Produces<Category>(StatusCodes.Status400BadRequest)
    ;
app.MapPost("/api/category", ([FromBody] Category model) =>
{
    if (model.Id != 0 || string.IsNullOrEmpty(model.Name))
    {
        return Results.BadRequest("Invalid");
    }
    if (CategoriesStore.Categories.Any(x => x.Name.ToLower() == model.Name.ToLower()))
    {
        return Results.BadRequest("Name already exists");
    }
    model.Id = CategoriesStore.Categories.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;
    model.CreatedBy = "x";
    model.CreatedOn = DateTime.Now;
    CategoriesStore.Categories.Add(model);
    return Results.CreatedAtRoute("getcategory", new { id = model.Id }, model);
    //return Results.Created($"/api/category/{model.Id}", model);
})
    .WithName("CreateCategory")
    .Produces<Category>(StatusCodes.Status201Created)
    .Produces<Category>(StatusCodes.Status400BadRequest)
    ;
app.MapPut("/api/category", () =>
{
});
app.MapDelete("/api/category/{id:int}", (int id) =>
{
});


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

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
