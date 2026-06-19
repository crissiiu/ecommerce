using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Infrastructure.Data;
using eCommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationApi.Infrastructure.Repositories
{
    public class UserRepository(AuthenticationDbContext context, IConfiguration config) : IUser
    {
        private async Task<AppUser> GetUserByEmail(string email)
        {
            var user = await context.User.FirstOrDefaultAsync(u => u.Email == email);
            return user is not null ? user : null!;
        }
        public async Task<GetUserDTO> GetUser(int UserId)
        {
            var user = await context.User.FindAsync(UserId);
            return user is not null ? new GetUserDTO(
                user.Id!,
                user.Name!,
                user.TelephoneNumber!,
                user.Address!,
                user.Email!,
                user.Role!) : null!;

        }

        public async Task<Response> Login(LoginDTO loginDTO)
        {
            var getUser = await GetUserByEmail(loginDTO.Email!);
            if (getUser is null)
            {
                return new Response(false, "Invalid Credentital");
            }

            bool verifyPassword = BCrypt.Net.BCrypt.Verify(loginDTO.Password, getUser.Password);
            if (!verifyPassword)
            {
                return new Response(false, "Invalid Credential");
            }

            string token = GeneralToken(getUser);
            return new Response(true, token);
        }

        public string GeneralToken(AppUser user)
        {
            var key = Encoding.UTF8.GetBytes(config.GetSection("Authentication:Key").Value!);
            var sercurityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(sercurityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Name!),
                new(ClaimTypes.Email, user.Email!)
            };

            if (string.IsNullOrEmpty(user.Role) || !Equals("string", user.Role))
            {
                claims.Add(new(ClaimTypes.Role, user.Role!));
            }

            var token = new JwtSecurityToken(
                issuer: config["Authentication:Issuer"],
                audience: config["Authentication:Audience"],
                claims: claims,
                expires: null,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Response> Register(AppUserDTO appUserDTO)
        {
            var getUser = await GetUserByEmail(appUserDTO.Email!);
            if(getUser is not null)
            {
                return new Response(false, "You cannot use this email for registeration");
            }

            var result = context.User.Add(new AppUser()
            {
                Name = appUserDTO.Name,
                Email = appUserDTO.Email,
                TelephoneNumber = appUserDTO.TelephoneNumber,
                Address = appUserDTO.Address,
                Password = BCrypt.Net.BCrypt.HashPassword(appUserDTO.Password),
                Role = appUserDTO.Role
            });

            await context.SaveChangesAsync();
            return result.Entity.Id > 0 ? new Response(true, "User registered successfully") : 
                new Response(false, "Invalid Data provided");
        }
    }
}
