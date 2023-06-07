using DriverAPI.Data;
using DriverAPI.DTOs.V1;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DriverAPI.Controllers.V1
{
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	public class TokenController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<TokenController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly double _tokenLifetime;
		private readonly string _securityKey;
		private bool _loginCaseSensitive;

		public TokenController(
			IConfiguration configuration,
			ILogger<TokenController> logger,
			ApplicationDbContext context,
			UserManager<IdentityUser> userManager)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

			_tokenLifetime = _configuration.GetValue<double>("Security:Token:Lifetime");
			_securityKey = _configuration.GetValue<string>("Security:Token:Key");
			_loginCaseSensitive = _configuration.GetValue("Security:User:LoginCaseSensitive", false);
		}

		[HttpPost]
		[Route("Authenticate")]
		[Route("/api/Authenticate")]
		public async Task<TokenResponseDto> Post([FromBody] LoginRequestDto loginRequestModel)
		{
			var userAgent = string.Empty;

			if(HttpContext.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgentHeader))
			{
				userAgent = userAgentHeader;
			}

			if(TryGetVersion(userAgent, out var driverApplicationVersion))
			{
				_logger.LogInformation("Запрошен токен для пользователя: {Username}, версия МП:{ApplicationVersion}", loginRequestModel.Username, driverApplicationVersion);
			}
			else
			{
				_logger.LogWarning("Запрошен токен для пользователя: {Username}, невозможно определить версию, User-Agent:{UserAgent}", loginRequestModel.Username, userAgent);
			}

			if(await IsValidCredentials(loginRequestModel.Username, loginRequestModel.Password))
			{
				return await GenerateToken(loginRequestModel.Username);
			}
			else
			{
				throw new UnauthorizedAccessException("Пара логин/пароль не найдена");
			}
		}

		private bool TryGetVersion(string useragent, out Version version)
		{
			var result = useragent;
			const string useragentPrefix = "Vodovoz_Driver/";
			var indexAfterVersion = useragent.IndexOf('+');

			var indexOfPrefix = useragent.IndexOf(useragentPrefix);

			if(indexOfPrefix < 0)
			{
				version = null;
				return false;
			}

			if(indexAfterVersion > 0)
			{
				result = useragent.Substring(0, indexAfterVersion);
			}

			result = result.Replace(useragentPrefix, string.Empty);

			return Version.TryParse(result, out version);
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
