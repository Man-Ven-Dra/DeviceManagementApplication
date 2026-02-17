using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeviceManagementAPI.Data;
using DeviceManagementAPI.DTO;
using DeviceManagementAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace DeviceManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IMongoCollection<Admin> _admins;
        private readonly IConfiguration _configuration;
        public AdminController(DeviceDbContext context, IConfiguration configuration)
        {
            _admins = context.Admins;
            _configuration = configuration;
        }

        private string GenerateJwtToken(Admin admin)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id),
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("signup")]
        public async Task<IActionResult> Signup(AdminSignupDto dto)
        {
            var isExisting = await _admins.Find(a => a.Email == dto.Email).FirstOrDefaultAsync();
            if(isExisting != null)
                return BadRequest("Email Already Registered!");

            var admin = new Admin
            {
                Username = dto.Username,
                Email = dto.Email
            };

            var passwordHasher = new PasswordHasher<Admin>();
            admin.HashedPassword = passwordHasher.HashPassword(admin, dto.Password);
            await _admins.InsertOneAsync(admin);

            return Ok("SignUp Successful!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto dto)
        {
            var admin = await _admins
                .Find(a => a.Email == dto.Email)
                .FirstOrDefaultAsync();

            if (admin == null)
                return Unauthorized("Invalid email or password");

            var passwordHasher = new PasswordHasher<Admin>();

            var passwordCheck = passwordHasher.VerifyHashedPassword(
                admin,
                admin.HashedPassword,
                dto.Password
            );

            if (passwordCheck == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password");

            var token = GenerateJwtToken(admin);

            return Ok("Login successful! "+ new {token});
        }
    }
}