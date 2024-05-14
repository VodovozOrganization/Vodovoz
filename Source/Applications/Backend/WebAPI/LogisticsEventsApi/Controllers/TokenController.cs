using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vodovoz.Presentation.WebApi.Security;

namespace LogisticsEventsApi.Controllers
{
	/// <summary>
	/// Контроллер аутентификации
	/// </summary>
	[ApiController]
	[Route("/api/[action]")]
	public class TokenController : AuthenticationController
	{
		public TokenController(
			IOptions<SecurityOptions> securityOptions,
			UserManager<IdentityUser> userManager)
			: base(securityOptions, userManager)
		{
		}
	}
}
