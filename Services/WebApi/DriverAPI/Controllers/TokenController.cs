using DriverAPI.Data;
using DriverAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TokenController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly double tokenLifetime;
		private readonly string securityKey;

		public TokenController(
			IConfiguration configuration,
			ApplicationDbContext context,
			UserManager<IdentityUser> userManager)
		{
			_configuration = configuration;
			_context = context;
			_userManager = userManager;

			tokenLifetime = _configuration.GetValue<double>("Security:Token:Lifetime");
			securityKey = _configuration.GetValue<string>("Security:Token:Key");
		}

		[HttpPost]
		[Route("/api/Authenticate")]
		public async Task<TokenResponseModel> Post([FromBody] LoginRequestModel loginRequestModel)
		{
			if (await IsValidCredentials(loginRequestModel.username, loginRequestModel.password))
			{
				return await GenerateToken(loginRequestModel.username);
			}
			else
			{
				throw new UnauthorizedAccessException("Пара логин/пароль не найдена");
			}
		}

		private async Task<bool> IsValidCredentials(string username, string password)
		{
			var user = await _userManager.FindByNameAsync(username);
			return await _userManager.CheckPasswordAsync(user, password);
		}

		private async Task<TokenResponseModel> GenerateToken(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			var roles = from ur in _context.UserRoles
						join r in _context.Roles on ur.RoleId equals r.Id
						where ur.UserId == user.Id
						select new { ur.UserId, ur.RoleId, r.Name };

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
				new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddMinutes(tokenLifetime)).ToUnixTimeSeconds().ToString())
			};

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role.Name));
			}

			var token = new JwtSecurityToken(
				new JwtHeader(
					new SigningCredentials(
						new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)),
						SecurityAlgorithms.HmacSha256)),
				new JwtPayload(claims));

			var output = new TokenResponseModel()
			{
				Access_Token = new JwtSecurityTokenHandler().WriteToken(token),
				UserName = username
			};

			return output;
		}
	}
}
