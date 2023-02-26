using DriverAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class AccountController : ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;

		public AccountController(UserManager<IdentityUser> userManager)
		{
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/api/Register")]
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

		[HttpPost]
		[AllowAnonymous]
		[Route("/api/ChangePassword")]
		public async Task ChangePassword([FromBody] PasswordDto password)
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			var user = await _userManager.GetUserAsync(User);

			var result = await _userManager.ChangePasswordAsync(user, password.CurrentPassword, password.NewPassword);

			if(!result.Succeeded)
			{
				throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
			}
		}

		public class PasswordDto
		{
			public string CurrentPassword { get; set; }
			public string NewPassword { get; set; }
		}
	}
}
