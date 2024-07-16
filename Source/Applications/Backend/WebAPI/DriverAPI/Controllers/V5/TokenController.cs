using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер аутентификации
	/// </summary>
	[ApiVersion("5.0")]
	public class TokenController : VersionedController
	{
		private readonly IConfiguration _configuration;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly double _tokenLifetime;
		private readonly string _securityKey;
		private bool _loginCaseSensitive;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="configuration"></param>
		/// <param name="userManager"></param>

		public TokenController(
			ILogger<TokenController> logger,
			IConfiguration configuration,
			UserManager<IdentityUser> userManager) : base(logger)
		{
			_configuration = configuration;
			_userManager = userManager;

			_tokenLifetime = _configuration.GetValue<double>("Security:Token:Lifetime");
			_securityKey = _configuration.GetValue<string>("Security:Token:Key");
			_loginCaseSensitive = _configuration.GetValue("Security:User:LoginCaseSensitive", false);
		}

		/// <summary>
		/// Аутентификация
		/// </summary>
		/// <param name="loginRequestModel"></param>
		/// <returns></returns>
		/// <exception cref="UnauthorizedAccessException"></exception>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
		public async Task<IActionResult> Authenticate([FromBody] LoginRequest loginRequestModel)
		{
			if(await IsValidCredentials(loginRequestModel.Username, loginRequestModel.Password))
			{
				return Ok(await GenerateToken(loginRequestModel.Username));
			}

			return Problem("Пара логин/пароль не найдена",
				statusCode: StatusCodes.Status401Unauthorized);
		}

		private async Task<bool> IsValidCredentials(string username, string password)
		{
			var user = await _userManager.FindByNameAsync(username);
			
			if(_loginCaseSensitive && user?.UserName != username)
			{
				return await new ValueTask<bool>(false);
			}

			return await _userManager.CheckPasswordAsync(user, password)
				&& await _userManager.IsInRoleAsync(user, ApplicationUserRole.Driver.ToString());
		}

		private async Task<TokenResponse> GenerateToken(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			var roles = await _userManager.GetRolesAsync(user);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
				new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddMinutes(_tokenLifetime)).ToUnixTimeSeconds().ToString())
			};
			
			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var token = new JwtSecurityToken(
				new JwtHeader(
					new SigningCredentials(
						new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey)),
						SecurityAlgorithms.HmacSha256)),
				new JwtPayload(claims));

			var output = new TokenResponse()
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
				UserName = username
			};

			return output;
		}
	}
}
