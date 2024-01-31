using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vodovoz.Core.Data.Dto_s;

namespace LogisticsEventsApi.Controllers
{
	/// <summary>
	/// Контроллер аутентификации
	/// </summary>
	[ApiController]
	public partial class TokenController : ControllerBase
	{
		private const string _key = "Key";
		private const string _lifetime = "Lifetime";
		private const string _loginCaseSensitive = "LoginCaseSensitive";
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IConfigurationSection _securityTokenSection;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="context"></param>
		/// <param name="userManager"></param>
		public TokenController(
			IConfiguration configuration,
			UserManager<IdentityUser> userManager)
		{
			_securityTokenSection = configuration.GetSection("SecurityToken");
			_userManager = userManager;
		}

		/// <summary>
		/// Аутентификация
		/// </summary>
		/// <param name="loginRequestModel"></param>
		/// <returns><see cref="TokenResponseDto"/></returns>
		[HttpPost("Authenticate")]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> AuthenticateAsync([FromBody] LoginRequestDto loginRequestModel)
		{
			if(await IsValidCredentialsAsync(loginRequestModel.Username, loginRequestModel.Password))
			{
				return Ok(await GenerateTokenAsync(loginRequestModel.Username));
			}

			return NotFound("Пара логин/пароль не найдена");
		}

		private async Task<bool> IsValidCredentialsAsync(string username, string password)
		{
			var user = await _userManager.FindByNameAsync(username);
			
			if(_securityTokenSection.GetValue(_loginCaseSensitive, false) && user?.UserName != username)
			{
				return await new ValueTask<bool>(false);
			}

			var userRoles = await _userManager.GetRolesAsync(user);
			var userHasAccessByRole = userRoles.Any(userRole => Startup.AccessedRoles.Contains(userRole));

			return await _userManager.CheckPasswordAsync(user, password) && userHasAccessByRole;
		}

		private async Task<TokenResponseDto> GenerateTokenAsync(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			var roles = await _userManager.GetRolesAsync(user);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
				new Claim(
					JwtRegisteredClaimNames.Exp,
					new DateTimeOffset(DateTime.Now.AddMinutes(_securityTokenSection.GetValue<int>(_lifetime)))
						.ToUnixTimeSeconds()
						.ToString()
					)
			};
			
			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var token = new JwtSecurityToken(
				new JwtHeader(
					new SigningCredentials(
						new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityTokenSection.GetValue<string>(_key))),
						SecurityAlgorithms.HmacSha256)),
				new JwtPayload(claims));

			var output = new TokenResponseDto
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
				UserName = username
			};

			return output;
		}
	}
}
