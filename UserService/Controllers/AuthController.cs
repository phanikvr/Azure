using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Data;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (await _userManager.FindByEmailAsync(model.Email) != null)
            return BadRequest(new AuthResponse { Message = "User already exists!" });

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = model.Email,
            Role = model.Role
        };
        try
        {
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(
                    new
                    {
                        token = (string?)null,
                        message = "User registration failed!",
                        errors = result.Errors.Select(e => e.Description).ToList()
                    }
                );

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return Ok(new { token, message = "User registered successfully!" });
        }
        catch (Exception ex)
        {
            // Log or return the error message
            return BadRequest(new { token = (string?)null, message = "Error: " + ex.Message });
        }

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        var token = GenerateJwtToken(user);
        return Ok(new AuthResponse { Token = token, Message = "Login successful!" });
    }

    // Generate JWT Token
    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("Role", user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}