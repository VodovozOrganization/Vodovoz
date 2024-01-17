using DriverAPI.DTOs.V4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер аккаунтов
	/// </summary>
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
			UserManager<IdentityUser> userManager)
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
		[HttpPost("Register")]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task Post([FromBody] RegisterRequestDto loginRequestModel)
		{
			var user = new IdentityUser
			{
				UserName = loginRequestModel.Username
			};

			try
			{
				var userCreatedResult = await _userManager.CreateAsync(user, loginRequestModel.Password);

				if(!userCreatedResult.Succeeded)
				{
					if(userCreatedResult.Errors.Any(e => e.Code == "DuplicateUserName"))
					{
						throw new ArgumentException("Имя пользователя уже занято");
					}

					throw new Exception(string.Join(", ", userCreatedResult.Errors.Select(e => e.Description)));
				}

				await AddRoleToUser(loginRequestModel);
			}
			catch
			{
				await _userManager.DeleteAsync(user);
				throw;
			}
		}
		
		/// <summary>
		/// Добавляет новую роль пользователю, если такой роли нет - создает, служебный
		/// </summary>
		/// <param name="loginRequestModel"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="Exception"></exception>
		[HttpPost("AddRoleToUser")]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task AddRoleToUser([FromBody] RegisterRequestDto loginRequestModel)
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
					
					return;
				}

				await _userManager.DeleteAsync(user);
				throw new Exception(string.Join(", ", roleAddedToUserResult.Errors.Select(e => e.Description)));
			}
		}
		
		/// <summary>
		/// Убирает роль у пользователя, служебный
		/// </summary>
		/// <param name="loginRequestModel"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="Exception"></exception>
		[HttpPost("RemoveRoleFromUser")]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task RemoveRoleFromUser([FromBody] RegisterRequestDto loginRequestModel)
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
					
					return;
				}
				
				throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
			}
		}
	}
}
