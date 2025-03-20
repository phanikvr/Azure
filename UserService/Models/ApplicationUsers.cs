using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace UserService.Data;
public record ApplicationUser 
{
    [JsonProperty("id")]
    public required string Id { get; set; } = Guid.NewGuid().ToString(); 
    public required string UserName { get; set; }
    public required string Role { get; set; }
    public string NormalizedUserName { get; set; }
    public string Email { get; set; }
    public string NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string PasswordHash { get; set; }
}

public record RegisterModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }

    public string Role { get; set; }
}

public record LoginModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public record AuthResponse
{
    public string Token { get; set; }
    public string Message { get; set; }
}
