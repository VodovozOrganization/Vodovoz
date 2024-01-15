using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LogisticsEventsApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LogisticsEventsApi.Controllers
{
	/// <summary>
	/// Контроллер аутентификации
	/// </summary>
	[ApiController]
	public class TokenController : ControllerBase
	{
		private const string _key = "Key";
		private const string _lifetime = "Lifetime";
		private const string _loginCaseSensitive = "LoginCaseSensitive";
		private readonly ApplicationDbContext _context;
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
			ApplicationDbContext context,
			UserManager<IdentityUser> userManager)
		{
			_securityTokenSection = configuration.GetSection("SecurityToken");
			_context = context ?? throw new ArgumentNullException(nameof(context));
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
			if(!_securityTokenSection.GetValue(_loginCaseSensitive, false) || user?.UserName == username)
			{
				return await _userManager.CheckPasswordAsync(user, password);
			}
			return await new ValueTask<bool>(false);
		}

		private async Task<TokenResponseDto> GenerateTokenAsync(string username)
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
				new Claim(
					JwtRegisteredClaimNames.Exp,
					new DateTimeOffset(DateTime.Now.AddMinutes(_securityTokenSection.GetValue<int>(_lifetime)))
						.ToUnixTimeSeconds()
						.ToString()
					)
			};

			foreach(var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role.Name));
			}

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
		
		/// <summary>
		/// Ответ сервера при успешной аутентификации
		/// </summary>
		public class TokenResponseDto
		{
			/// <summary>
			/// Access - токен (JWT)
			/// </summary>
			public string AccessToken { get; set; }

			/// <summary>
			/// Логин пользователя
			/// </summary>
			public string UserName { get; set; }
		}
		
		/// <summary>
		/// Учетные данные для авторизации
		/// </summary>
		public class LoginRequestDto
		{
			/// <summary>
			/// Логин пользователя
			/// </summary>
			[Required]
			public string Username { get; set; }

			/// <summary>
			/// Пароль пользователя
			/// </summary>
			[Required]
			public string Password { get; set; }
		}
	}
}
