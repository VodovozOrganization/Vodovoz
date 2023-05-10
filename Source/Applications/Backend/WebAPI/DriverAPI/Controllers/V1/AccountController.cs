using DriverAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DriverAPI.Controllers.V1
{
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}")]
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
		[Route("Register")]
		[Route("/api/Register")]
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
