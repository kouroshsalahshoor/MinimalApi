﻿using Microsoft.AspNetCore.Identity;

namespace MinimalApi.Models.Auth;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
