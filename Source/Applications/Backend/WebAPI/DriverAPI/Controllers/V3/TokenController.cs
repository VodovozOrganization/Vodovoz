using DriverAPI.Data;
using DriverAPI.DTOs.V2;
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

namespace DriverAPI.Controllers.V3
{
	[ApiVersion("3.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	public class TokenController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly double _tokenLifetime;
		private readonly string _securityKey;
		private bool _loginCaseSensitive;

		public TokenController(
			IConfiguration configuration,
			ApplicationDbContext context,
			UserManager<IdentityUser> userManager)
		{
			_configuration = configuration;
			_context = context;
			_userManager = userManager;

			_tokenLifetime = _configuration.GetValue<double>("Security:Token:Lifetime");
			_securityKey = _configuration.GetValue<string>("Security:Token:Key");
			_loginCaseSensitive = _configuration.GetValue("Security:User:LoginCaseSensitive", false);
		}

		[HttpPost]
		[Route("Authenticate")]
		public async Task<TokenResponseDto> Post([FromBody] LoginRequestDto loginRequestModel)
		{
			if(await IsValidCredentials(loginRequestModel.Username, loginRequestModel.Password))
			{
				return await GenerateToken(loginRequestModel.Username);
			}
			else
			{
				throw new UnauthorizedAccessException("Пара логин/пароль не найдена");
			}
		}

		private async Task<bool> IsValidCredentials(string username, string password)
		{
			var user = await _userManager.FindByNameAsync(username);
			if(!_loginCaseSensitive || user?.UserName == username)
			{
				return await _userManager.CheckPasswordAsync(user, password);
			}
			return await new ValueTask<bool>(false);
		}

		private async Task<TokenResponseDto> GenerateToken(string username)
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
				new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddMinutes(_tokenLifetime)).ToUnixTimeSeconds().ToString())
			};

			foreach(var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role.Name));
			}

			var token = new JwtSecurityToken(
				new JwtHeader(
					new SigningCredentials(
						new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey)),
						SecurityAlgorithms.HmacSha256)),
				new JwtPayload(claims));

			var output = new TokenResponseDto()
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
				UserName = username
			};

			return output;
		}
	}
}
