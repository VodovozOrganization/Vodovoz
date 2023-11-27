using DriverAPI.DTOs.V4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер аккаунтов
	/// </summary>
	[Authorize]
	public class AccountController : VersionedController
	{
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="userManager"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public AccountController(UserManager<IdentityUser> userManager)
		{
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
		[Route("Register")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task Post([FromBody] RegisterRequestDto loginRequestModel)
		{
			var user = new IdentityUser() { UserName = loginRequestModel.Username };
			var result = await _userManager.CreateAsync(user, loginRequestModel.Password);

			if(!result.Succeeded)
			{
				if(result.Errors.Any(e => e.Code == "DuplicateUserName"))
				{
					throw new ArgumentException("Имя пользователя уже занято");
				}

				throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
			}
		}
	}
}
