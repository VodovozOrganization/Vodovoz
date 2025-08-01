using MassTransit.Initializers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;

namespace Vodovoz.Presentation.WebApi.Security
{
	/// <summary>
	/// Контроллер аутентификации
	/// </summary>
	[ApiController]
	[Route("/api/[action]")]
	public abstract class AuthenticationController : ControllerBase
	{
		private readonly IOptions<SecurityOptions> _securityOptions;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IFirebaseCloudMessagingService _firebaseCloudMessagingService;
		private readonly IUnitOfWork _unitOfWork;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="context"></param>
		/// <param name="userManager"></param>
		public AuthenticationController(
			IOptions<SecurityOptions> securityOptions,
			UserManager<IdentityUser> userManager,
			IFirebaseCloudMessagingService firebaseCloudMessagingService,
			IUnitOfWork unitOfWork)
		{
			_securityOptions = securityOptions
				?? throw new ArgumentNullException(nameof(securityOptions));
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_firebaseCloudMessagingService = firebaseCloudMessagingService
				?? throw new ArgumentNullException(nameof(firebaseCloudMessagingService));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		/// <summary>
		/// Аутентификация
		/// </summary>
		/// <param name="loginRequestModel"></param>
		/// <returns><see cref="TokenResponseDto"/></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> AuthenticateAsync(LoginRequest loginRequestModel)
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

			if(user is null)
			{
				return await ValueTask.FromResult(false);
			}

			if((_securityOptions.Value.User?.LoginCaseSensitive ?? false)
				&& user?.UserName != username)
			{
				return await ValueTask.FromResult(false);
			}

			var userRolesStrings = await _userManager.GetRolesAsync(user);

			var userRoles = userRolesStrings
				.Select(x =>
				{
					if(x is null)
					{
						return null;
					}

					if(Enum.TryParse(typeof(ApplicationUserRole), x, out var role))
					{
						return role;
					}

					return null;
				})
				.Where(x => x != null)
				.Cast<ApplicationUserRole>();

			var userHasAccessByRole =
				!(_securityOptions.Value?.Authorization?.GrantedRoles.Any() ?? false)
				|| _securityOptions.Value.Authorization.GrantedRoles
					.Any(x => userRoles.Contains(x));

			return await _userManager.CheckPasswordAsync(user, password) && userHasAccessByRole;
		}

		private async Task<TokenResponse> GenerateTokenAsync(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			var roles = await _userManager.GetRolesAsync(user);

			var lifetimeOffset = _securityOptions.Value.Token?.Lifetime ?? TimeSpan.FromHours(1);

			var activeSessionKey = Guid.NewGuid().ToString();

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
				new Claim(
					JwtRegisteredClaimNames.Exp,
					DateTimeOffset.Now.Add(lifetimeOffset)
						.ToUnixTimeSeconds()
						.ToString()),
				new Claim(VodovozClaimTypes.ActiveSessionKey, activeSessionKey)
			};

			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var token = new JwtSecurityToken(
				new JwtHeader(
					new SigningCredentials(
						new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityOptions.Value.Token.Key)),
						SecurityAlgorithms.HmacSha256)),
				new JwtPayload(claims));

			var response = new TokenResponse
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
				UserName = username
			};

			if(_securityOptions.Value.Authorization.OnlyOneSessionAllowed)
			{
				var externalApplicationUser = _unitOfWork.Session
					.Query<ExternalApplicationUserForApi>()
					.Where(eau => eau.Login == username
						&& _securityOptions.Value.Authorization.ApplicationUserTypes.Contains(eau.ExternalApplicationType))
					.FirstOrDefault();

				if(!string.IsNullOrWhiteSpace(externalApplicationUser?.Token))
				{
					await _firebaseCloudMessagingService
						.SendMessage(
							externalApplicationUser.Token,
							"Веселый водовоз",
							"Выполнен вход на другом устройстве");
				}

				externalApplicationUser.SessionKey = activeSessionKey;

				_unitOfWork.Save(externalApplicationUser);
				_unitOfWork.Commit();
			}

			return response;
		}
	}
}
