using DriverApi.Contracts.V5.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер аккаунтов
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize]
	public class AccountController : VersionedController
	{
		private readonly ILogger<AccountController> _logger;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="roleManager"></param>
		/// <param name="userManager"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public AccountController(
			ILogger<AccountController> logger,
			RoleManager<IdentityRole> roleManager,
			UserManager<IdentityUser> userManager) : base(logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		/// <summary>
		/// Регистрирует новых пользователей, служебный
		/// </summary>
		/// <param name="loginRequestModel"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="Exception"></exception>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Register([FromBody] RegisterRequest loginRequestModel)
		{
			var user = new IdentityUser
			{
				UserName = loginRequestModel.Username
			};

			IdentityResult userCreatedResult = null;

			try
			{
				userCreatedResult = await _userManager.CreateAsync(user, loginRequestModel.Password);

				if(!userCreatedResult.Succeeded)
				{
					if(userCreatedResult.Errors.Any(e => e.Code == "DuplicateUserName"))
					{
						_logger.LogWarning("Имя пользователя {UserName} уже занято", loginRequestModel.Username);
						
						return Problem("Имя пользователя уже занято", statusCode: StatusCodes.Status400BadRequest);
					}

					_logger.LogError("Произошел ряд ошибок при создании пользователя: {ExceptionMessages}", string.Join(", ", userCreatedResult.Errors.Select(e => e.Description)));

					return Problem("Не удалось зарегистрировать пользователя");
				}

				await AddRoleToUser(loginRequestModel);

				return NoContent();
			}
			catch(Exception ex)
			{
				if(userCreatedResult != null && userCreatedResult.Succeeded)
				{
					await _userManager.DeleteAsync(user);
				}

				_logger.LogError(ex, "Произошла ошибка при создании пользователя: {ExceptionMessage}", ex.Message);

				return Problem("Не удалось зарегистрировать пользователя");
			}
		}
		
		/// <summary>
		/// Добавляет новую роль пользователю, если такой роли нет - создает, служебный
		/// </summary>
		/// <param name="loginRequestModel"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="Exception"></exception>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> AddRoleToUser([FromBody] RegisterRequest loginRequestModel)
		{
			var user = await _userManager.FindByNameAsync(loginRequestModel.Username);
			
			if(!await _roleManager.RoleExistsAsync(loginRequestModel.UserRole))
			{
				await _roleManager.CreateAsync(new IdentityRole
				{
					Name = loginRequestModel.UserRole
				});
			}
			
			var roleAddedToUserResult = await _userManager.AddToRoleAsync(user, loginRequestModel.UserRole);

			if(!roleAddedToUserResult.Succeeded)
			{
				if(roleAddedToUserResult.Errors.Any(x => x.Code == "UserAlreadyInRole"))
				{
					_logger.LogWarning(
						"Попытка добавления пользователю {User} роли {Role}, которая у него уже есть",
						user.UserName,
						loginRequestModel.UserRole);
					
					return NoContent();
				}

				_logger.LogError("Произошел ряд ошибок при добавлении роли: {ExceptionMessages}", string.Join(", ", roleAddedToUserResult.Errors.Select(e => e.Description)));

				return Problem("Не удалось добавить роль");
			}

			return NoContent();
		}

		/// <summary>
		/// Убирает роль у пользователя, служебный
		/// </summary>
		/// <param name="loginRequestModel"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="Exception"></exception>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> RemoveRoleFromUser([FromBody] RegisterRequest loginRequestModel)
		{
			var user = await _userManager.FindByNameAsync(loginRequestModel.Username);
			var result = await _userManager.RemoveFromRoleAsync(user, loginRequestModel.UserRole);
			
			if(!result.Succeeded)
			{
				if(result.Errors.Any(x => x.Code == "UserNotInRole"))
				{
					_logger.LogWarning(
						"Пользователь {User} не имеет активной роли {Role}",
						user.UserName,
						loginRequestModel.UserRole);

					return NoContent();
				}

				_logger.LogError("Произошел ряд ошибок при удалении роли: {ExceptionMessages}", string.Join(", ", result.Errors.Select(e => e.Description)));
				
				return Problem("Не удалось удалить роль");
			}

			return NoContent();
		}
	}
}
