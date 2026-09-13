using ExceptionHandling.Data;
using ExceptionHandling.DTO;
using ExceptionHandling.Exceptions;
using ExceptionHandling.Models;
using ExceptionHandling.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthApi.Services;

public class AuthService(
    ApplicationDbContext context,IConfiguration configuration,
    UserManager<User> userManager,
    RoleManager<IdentityRole<int>> roleManager,
    SignInManager<User> signInManager)
    : IAuthService
{
    public async Task RegisterAsync(RegisterDto dto)
    {
        //Ստուգում ենք արդյոք Email արդեն կա թե ոչ
        var userExist = await userManager.FindByEmailAsync(dto.Email);

        //Ստում User Մութքագրած Email ը կա արդյոք բազայում թո ոչ 
        if (userExist is not null)
            throw new ConflictException("Email already exists.");

        //Սա դեռ database-ում User չի ստեղծում։
        var user = new User
        {
            UserName = dto.Email,
            Email= dto.Email
        };
        var result = await userManager.CreateAsync(user,dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ",result.Errors.Select(x => x.Description));
            throw new BadRequestException(errors);
        }
        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole<int>("User"));

        var roleResult = await userManager.AddToRoleAsync(user,"User");
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(x => x.Description));
            throw new BadRequestException(errors);
        }
    }
    // =========================
    // LOGIN
    // =========================
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");
        var result = await signInManager.CheckPasswordSignInAsync(user,
               dto.Password,
               lockoutOnFailure: true);
        if (result.IsLockedOut)
            throw new UnauthorizedException(
                "Account is locked.");
        if (!result.Succeeded)
            throw new UnauthorizedException("Invalid email or password.");

        var accessToken = await GenerateJwtToken(user);

        // Create Refresh Token
        var refreshToken = GenerateRefreshToken();

        var refreshTokenHash = HashRefreshToken(refreshToken);
     
        // Save Refresh Token in Database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")),
            IsRevoked = false
        };
        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync();

        // Return both tokens
        return new LoginResponseDto(
            accessToken,
            refreshToken
        );
    }
    // =========================
    // REFRESH TOKEN
    // =========================

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var refreshTokenHash = HashRefreshToken(dto.RefreshToken);

        var refreshToken = await context.RefreshTokens
                .Include(x => x.User).FirstOrDefaultAsync(x => x.Token == refreshTokenHash);
        if (refreshToken is null)
            throw new UnauthorizedException("Invalid refresh token");
        
        if (refreshToken.IsRevoked)
            throw new UnauthorizedException("Refresh token has been revoked");

        if (refreshToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token has expired");

        var newAccessToken = await GenerateJwtToken(refreshToken.User);
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenHash = HashRefreshToken(newRefreshToken);
        refreshToken.IsRevoked = true;
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshTokenHash,
            UserId = refreshToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")),
            IsRevoked = false
        };
        context.RefreshTokens.Add(newRefreshTokenEntity);
        await context.SaveChangesAsync();
        return new LoginResponseDto(
            newAccessToken,
            newRefreshToken
        );
    }
    // =========================
    // GENERATE JWT
    // =========================
    private async Task<string> GenerateJwtToken(User user)
    {
        var expirationMinutes =configuration.GetValue<int>("Jwt:ExpirationMinutes");

          var roles = await userManager.GetRolesAsync(user);

          var claims = new List<Claim>
         {
             new(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
            new(ClaimTypes.Email,user.Email!)
         };
        foreach (var role in roles)
        {
            claims.Add(
                new Claim(ClaimTypes.Role,role)
            );
        }
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                expirationMinutes
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
    // =========================
    // GENERATE REFRESH TOKEN
    // =========================
    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }
    private string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hash);
    }
    public async Task LogoutAsync(RefreshTokenDto dto)
    {
        var refreshTokenHash = HashRefreshToken(dto.RefreshToken);
        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshTokenHash);
        if (refreshToken is null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }
        refreshToken.IsRevoked = true;
        await context.SaveChangesAsync();
    }

}
