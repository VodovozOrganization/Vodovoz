using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Presentation.WebApi.Security;

namespace WarehouseApi.Controllers.V1
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
			UserManager<IdentityUser> userManager,
			IFirebaseCloudMessagingService firebaseCloudMessagingService,
			IUnitOfWork unitOfWork)
			: base(securityOptions, userManager, firebaseCloudMessagingService, unitOfWork)
		{
		}
	}
}
