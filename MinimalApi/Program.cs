using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using MinimalApi.Data;
using MinimalApi.Models;
using MinimalApi.Models.Dtos;
using MinimalApi.Utilities;
using MinimalApi.Validations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
//builder.Services.AddScoped<IValidator<CategoryCreateDto>, CategoryCreateValidation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/categories", (ILogger<Program> _logger, IMapper _mapper) =>
{
    _logger.Log(LogLevel.Information, ">>> Get /api/categories");
    var models = CategoriesStore.Categories.ToList();

    return Results.Ok(_mapper.Map<List<CategoryDto>>(models));
}).WithName("GetCategories")
        .Produces<List<CategoryDto>>(StatusCodes.Status200OK)
        ;
app.MapGet("/api/category/{id:int}", (int id, ILogger<Program> _logger, IMapper _mapper) =>
{
    _logger.Log(LogLevel.Information, $">>> Get /api/category/{id}");

    if (id < 1)
    {
        return Results.BadRequest("Invalid");
    }

    var model = CategoriesStore.Categories.FirstOrDefault(x => x.Id == id);
    if (model == null)
        return Results.NotFound();
    return Results.Ok(_mapper.Map<CategoryDto>(model));
})
    .WithName("GetCategory")
    .Produces<CategoryDto>(StatusCodes.Status200OK)
    .Produces<CategoryDto>(StatusCodes.Status400BadRequest)
    ;
app.MapPost("/api/category", async (
    [FromBody] CategoryCreateDto dto,
    ILogger < Program > _logger,
    IMapper _mapper,
    IValidator<CategoryCreateDto> _validator
    ) =>
{
    _logger.Log(LogLevel.Information, $">>> Post /api/category");

    var validationResult = await _validator.ValidateAsync(dto);
    if (validationResult.IsValid == false)
    {
        return Results.BadRequest(validationResult.Errors.FirstOrDefault()!.ToString());
    }

    if (string.IsNullOrEmpty(dto.Name))
    {
        return Results.BadRequest("Invalid");
    }
    if (CategoriesStore.Categories.Any(x => x.Name.ToLower() == dto.Name.ToLower()))
    {
        return Results.BadRequest("Name already exists");
    }

    var model = _mapper.Map<Category>(dto);

    model.Id = CategoriesStore.Categories.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;
    model.CreatedBy = "x";
    model.CreatedOn = DateTime.Now;
    CategoriesStore.Categories.Add(model);

    var categoryDto = _mapper.Map<CategoryDto>(model);

    return Results.CreatedAtRoute("getcategory", new { id = categoryDto.Id }, categoryDto);
    //return Results.Created($"/api/category/{model.Id}", model);
})
    .WithName("CreateCategory")
    .Accepts<CategoryCreateDto>("application/json")
    .Produces<CategoryDto>(StatusCodes.Status201Created)
    .Produces<CategoryDto>(StatusCodes.Status400BadRequest)
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
