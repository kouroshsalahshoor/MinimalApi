using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Repository.IRepository;
using MinimalApi.Utilities;
using System.Net;
using MinimalApi.Models.Dtos.Auth;
using Newtonsoft.Json;
using MinimalApi.Models.Dtos;

namespace MinimalApi.Endpoints;

public static class AuthEndpoints
{
    public static void ConfigureAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/register", register).WithName("Register").Accepts<RegisterDto>("application/json")
            .Produces<ApiResponse>(StatusCodes.Status200OK).Produces<ApiResponse>(StatusCodes.Status400BadRequest);
        app.MapPost("/api/login", login).WithName("Login").Accepts<LoginDto>("application/json")
            .Produces<ApiResponse>(StatusCodes.Status200OK).Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private async static Task<IResult> register(
        IAuthRepository _repo,
        [FromBody] RegisterDto dto,
        ILogger<Program> _logger)
    {
        _logger.Log(LogLevel.Information, $">>> Post /api/register");

        var apiResponse = new ApiResponse();
        apiResponse.IsSuccessful = false;
        apiResponse.StatusCode = HttpStatusCode.BadRequest;

        if (await _repo.IsUnique(dto.UserName) == false) {
            apiResponse.Errors.Add("Username already exists!");
            return Results.BadRequest(apiResponse);
        }

        var response = await _repo.Register(dto);
        if (response == null || response.IsSuccessful == false)
        {
            apiResponse.Errors = response != null ? response!.Errors : new List<string>() { "Error register" };
            return Results.BadRequest(apiResponse);
        }

        apiResponse.IsSuccessful = true;
        apiResponse.StatusCode = HttpStatusCode.OK;
        apiResponse.Result = response.User;

        return Results.Ok(apiResponse);
    }
    private async static Task<IResult> login(
        IAuthRepository _repo,
        [FromBody] LoginDto dto,
        ILogger<Program> _logger)
    {
        _logger.Log(LogLevel.Information, $">>> Post /api/login");

        var apiResponse = new ApiResponse();
        apiResponse.IsSuccessful = false;
        apiResponse.StatusCode = HttpStatusCode.BadRequest;

        var response = await _repo.Login(dto);
        if (response == null)
        {
            apiResponse.Errors.Add("Invalid Login!");
            return Results.BadRequest(apiResponse);
        }

        apiResponse.IsSuccessful = true;
        apiResponse.StatusCode = HttpStatusCode.OK;
        apiResponse.Result = response;

        return Results.Ok(apiResponse);
    }
}
