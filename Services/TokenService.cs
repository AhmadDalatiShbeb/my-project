using Ticket_Booking_System.Model; using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Ticket_Booking_System.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager; 
        public TokenService(IConfiguration config, UserManager<User> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public string GenerateJwtToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),                 new Claim(JwtRegisteredClaimNames.Email, user.Email!)             };

                        claims.Add(new Claim("FirstName", user.FirstName));
            claims.Add(new Claim("LastName", user.LastName));

                        foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        if (!int.TryParse(_config["Jwt:ExpireDays"], out int expireDays))
            {
                expireDays = 7;             }

            var expires = DateTime.Now.AddDays(Convert.ToDouble(_config["Jwt:ExpireDays"] ?? "7")); 
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}