using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using MinimalApi.Data;
using MinimalApi.Models;
using MinimalApi.Models.Dtos;
using MinimalApi.Repository;
using MinimalApi.Repository.IRepository;
using MinimalApi.Utilities;
using MinimalApi.Validations;
using System.Net;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
//builder.Services.AddScoped<IValidator<CategoryCreateDto>, CategoryCreateValidation>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/categories", async (ICategoryRepository _repo, ILogger<Program> _logger, IMapper _mapper) =>
{
    _logger.Log(LogLevel.Information, ">>> Get /api/categories");
    var models = await _repo.Get();

    var apiResponse = new ApiResponse();
    apiResponse.IsSuccessful = true;
    apiResponse.Result = _mapper.Map<List<CategoryDto>>(models);
    apiResponse.StatusCode = HttpStatusCode.OK;

    return Results.Ok(apiResponse);
}).WithName("GetCategories")
        .Produces<List<ApiResponse>>(StatusCodes.Status200OK)
        ;
app.MapGet("/api/category/{id:int}", async (ICategoryRepository _repo, int id, ILogger<Program> _logger, IMapper _mapper) =>
{
    _logger.Log(LogLevel.Information, $">>> Get /api/category/{id}");

    var apiResponse = new ApiResponse();
    apiResponse.IsSuccessful = false;
    if (id < 1)
    {
        apiResponse.StatusCode = HttpStatusCode.BadRequest;
        apiResponse.Errors.Add("Id is zero or negative");
        return Results.BadRequest(apiResponse);
    }
    else
    {
        var model = await _repo.Get(id);
        if (model == null)
        {
            apiResponse.StatusCode = HttpStatusCode.NotFound;
            return Results.NotFound(apiResponse);
        }
        else
        {
            apiResponse.IsSuccessful = true;
            apiResponse.Result = _mapper.Map<CategoryDto>(model);
            apiResponse.StatusCode = HttpStatusCode.OK;
        }
    }

    return Results.Ok(apiResponse);
})
    .WithName("GetCategory")
    .Produces<ApiResponse>(StatusCodes.Status200OK)
    .Produces<ApiResponse>(StatusCodes.Status404NotFound)
    .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
    ;
app.MapPost("/api/category", async (
    ICategoryRepository _repo,
    [FromBody] CategoryCreateDto dto,
    ILogger<Program> _logger,
    IMapper _mapper,
    IValidator<CategoryCreateDto> _validator
    ) =>
{
    _logger.Log(LogLevel.Information, $">>> Post /api/category");

    var apiResponse = new ApiResponse();
    apiResponse.IsSuccessful = false;
    apiResponse.StatusCode = HttpStatusCode.BadRequest;

    var validationResult = await _validator.ValidateAsync(dto);
    if (validationResult.IsValid == false)
    {
        apiResponse.Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
        return Results.BadRequest(apiResponse);
    }

    if (string.IsNullOrEmpty(dto.Name))
    {
        apiResponse.Errors.Add("Invalid");
        return Results.BadRequest(apiResponse);
    }
    if ((await _repo.Get(dto.Name)) != null)
    {
        apiResponse.Errors.Add("Name already exists");
        return Results.BadRequest(apiResponse);
    }

    var model = _mapper.Map<Category>(dto);

    model.CreatedBy = "x";
    model.CreatedOn = DateTime.Now;
    await _repo.Create(model);

    var categoryDto = _mapper.Map<CategoryDto>(model);

    apiResponse.IsSuccessful = true;
    apiResponse.StatusCode = HttpStatusCode.Created;
    apiResponse.Result = categoryDto;

    return Results.Ok(apiResponse);
    //return Results.CreatedAtRoute("getcategory", new { id = categoryDto.Id }, categoryDto);
    //return Results.Created($"/api/category/{model.Id}", model);
})
    .WithName("CreateCategory")
    .Accepts<CategoryCreateDto>("application/json")
    .Produces<ApiResponse>(StatusCodes.Status200OK)
    //.Produces<ApiResponse>(StatusCodes.Status201Created)
    .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
    ;
app.MapPut("/api/category", async (
    ICategoryRepository _repo,
    [FromBody] CategoryUpdateDto dto,
    ILogger<Program> _logger,
    IMapper _mapper,
    IValidator<CategoryUpdateDto> _validator
    ) =>
{
    _logger.Log(LogLevel.Information, $">>> Put /api/category");

    var apiResponse = new ApiResponse();
    apiResponse.IsSuccessful = false;
    apiResponse.StatusCode = HttpStatusCode.BadRequest;

    var validationResult = await _validator.ValidateAsync(dto);
    if (validationResult.IsValid == false)
    {
        apiResponse.Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
        return Results.BadRequest(apiResponse);
    }

    if (string.IsNullOrEmpty(dto.Name))
    {
        apiResponse.Errors.Add("Invalid");
        return Results.BadRequest(apiResponse);
    }
    if ((await _repo.Get()).Any(x => x.Id != dto.Id && x.Name.ToLower() == dto.Name.ToLower()))
    {
        apiResponse.Errors.Add("Name already exists");
        return Results.BadRequest(apiResponse);
    }

    var model = await _repo.Get(dto.Id);

    model!.Name = dto.Name;
    model.LastModifiedBy = "x";
    model.LastModifiedOn = DateTime.Now;

    await _repo.Update(model);

    apiResponse.IsSuccessful = true;
    apiResponse.StatusCode = HttpStatusCode.OK;
    apiResponse.Result = _mapper.Map<CategoryDto>(model);

    return Results.Ok(apiResponse);
})
    .WithName("UpdateCategory")
    .Accepts<CategoryUpdateDto>("application/json")
    .Produces<ApiResponse>(StatusCodes.Status200OK)
    .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
    ;
app.MapDelete("/api/category/{id:int}", async (ICategoryRepository _repo, int id, ILogger<Program> _logger, IMapper _mapper) =>
{
    _logger.Log(LogLevel.Information, $">>> Delete /api/category/{id}");

    var apiResponse = new ApiResponse();
    apiResponse.IsSuccessful = false;
    if (id < 1)
    {
        apiResponse.StatusCode = HttpStatusCode.BadRequest;
        apiResponse.Errors.Add("Id is zero or negative");
        return Results.BadRequest(apiResponse);
    }
    else
    {
        var model = await _repo.Get(id);
        if (model == null)
        {
            apiResponse.StatusCode = HttpStatusCode.NotFound;
            apiResponse.Errors.Add("Invalid Id");
            return Results.NotFound(apiResponse);
        }
        else
        {
            await _repo.Delete(model);

            apiResponse.IsSuccessful = true;
            apiResponse.Result = _mapper.Map<CategoryDto>(model);
            apiResponse.StatusCode = HttpStatusCode.NoContent;
            return Results.Ok(apiResponse);
        }
    }
})
    .WithName("DeleteCategory")
    .Produces<ApiResponse>(StatusCodes.Status204NoContent)
    .Produces<ApiResponse>(StatusCodes.Status404NotFound)
    .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
    ;


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
