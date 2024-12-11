using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Models.Dtos;
using MinimalApi.Models;
using MinimalApi.Repository.IRepository;
using MinimalApi.Utilities;
using System.Net;
using System.Net.NetworkInformation;

namespace MinimalApi.Endpoints;

public static class CategoryEndpoints
{
    public static void ConfigureCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/categories", getAll).WithName("GetCategories")
        .Produces<List<ApiResponse>>(StatusCodes.Status200OK)
        .RequireAuthorization();

        app.MapGet("/api/category/{id:int}", get).WithName("GetCategory")
            .Produces<ApiResponse>(StatusCodes.Status200OK).Produces<ApiResponse>(StatusCodes.Status404NotFound).Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        app.MapPost("/api/category", create).WithName("CreateCategory").Accepts<CategoryCreateDto>("application/json")
            .Produces<ApiResponse>(StatusCodes.Status200OK) //.Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        app.MapPut("/api/category", update).WithName("UpdateCategory").Accepts<CategoryUpdateDto>("application/json")
            .Produces<ApiResponse>(StatusCodes.Status200OK).Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        app.MapDelete("/api/category/{id:int}", delete).WithName("DeleteCategory")
            .Produces<ApiResponse>(StatusCodes.Status204NoContent).Produces<ApiResponse>(StatusCodes.Status404NotFound).Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> getAll(ICategoryRepository _repo, ILogger<Program> _logger, IMapper _mapper)
    {
        _logger.Log(LogLevel.Information, ">>> Get /api/categories");
        var models = await _repo.Get();

        var apiResponse = new ApiResponse();
        apiResponse.IsSuccessful = true;
        apiResponse.Result = _mapper.Map<List<CategoryDto>>(models);
        apiResponse.StatusCode = HttpStatusCode.OK;

        return Results.Ok(apiResponse);
    }

    private static async Task<IResult> get(ICategoryRepository _repo, int id, ILogger<Program> _logger, IMapper _mapper)
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
    }

    private static async Task<IResult> create(
            ICategoryRepository _repo,
            [FromBody] CategoryCreateDto dto,
            ILogger<Program> _logger,
            IMapper _mapper,
            IValidator<CategoryCreateDto> _validator
            )
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
    }

    private static async Task<IResult> update(
            ICategoryRepository _repo,
            [FromBody] CategoryUpdateDto dto,
            ILogger<Program> _logger,
            IMapper _mapper,
            IValidator<CategoryUpdateDto> _validator
            )
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
    }

    private static async Task<IResult> delete(ICategoryRepository _repo, int id, ILogger<Program> _logger, IMapper _mapper)
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
    }
}
