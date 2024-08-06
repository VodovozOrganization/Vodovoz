﻿using LogisticsEventsApi.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using QS.DomainModel.UoW;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Security;

namespace LogisticsEventsApi.Controllers
{
	[Route("api/[action]")]
	[ApiController]
	public class PushNotificationsController : ApiControllerBase
	{
		private readonly ILogger<PushNotificationsController> _logger;
		private readonly IOptions<SecurityOptions> _securityOptions;
		private readonly IGenericRepository<ExternalApplicationUserForApi> _externalApplicationUserRepository;
		private readonly UserManager<IdentityUser> _userManager;

		public PushNotificationsController(
			ILogger<PushNotificationsController> logger,
			IOptions<SecurityOptions> securityOptions,
			IGenericRepository<ExternalApplicationUserForApi> externalApplicationUserRepository,
			UserManager<IdentityUser> userManager)
			: base(logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_securityOptions = securityOptions;
			_externalApplicationUserRepository = externalApplicationUserRepository;
			_userManager = userManager ?? throw new System.ArgumentNullException(nameof(userManager));
		}

		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> EnablePushNotificationsAsync([FromServices] IUnitOfWork unitOfWork, EnablePushNotificationsRequest enablePushNotificationsRequest)
		{
			_logger.LogInformation("Запрошена подписка на PUSH-сообщения для пользователя {Username} Firebase token: {FirebaseToken}, User token: {AccessToken}",
			User.Identity?.Name ?? "Unknown",
			enablePushNotificationsRequest.Token,
			Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);

			if(user is null)
			{
				return Problem("Пользователь {Username} не найден, невозможно подписаться на пуш сообщения", User.Identity?.Name);
			}

			var externalApplicationUser = _externalApplicationUserRepository.Get(
				unitOfWork,
				eau => eau.Login == user.UserName
					&& _securityOptions.Value.Authorization.ApplicationUserTypes.Contains(eau.ExternalApplicationType))
				.FirstOrDefault();

			if(externalApplicationUser is null)
			{
				return Problem("Пользователь приложения склада {Username} не найден, невозможно подписаться на пуш сообщения", user.UserName);
			}

			externalApplicationUser.Token = enablePushNotificationsRequest.Token;

			unitOfWork.Save(externalApplicationUser);
			unitOfWork.Commit();
			unitOfWork.Dispose();

			return NoContent();
		}

		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DisablePushNotificationsAsync([FromServices] IUnitOfWork unitOfWork)
		{
			_logger.LogInformation("Запрошена отписка от PUSH-сообщений для пользователя {Username} User token: {AccessToken}",
				User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);

			if(user is null)
			{
				return Problem("Пользователь {Username} не найден, невозможно отписаться от пуш сообщений", User.Identity?.Name);
			}

			var externalApplicationUser = _externalApplicationUserRepository.Get(
				unitOfWork,
				eau => eau.Login == user.UserName
					&& _securityOptions.Value.Authorization.ApplicationUserTypes.Contains(eau.ExternalApplicationType))
				.FirstOrDefault();

			if(externalApplicationUser is null)
			{
				return Problem("Пользователь приложения склада {Username} не найден, невозможно отписаться от пуш сообщений", user.UserName);
			}

			externalApplicationUser.Token = null;

			unitOfWork.Save(externalApplicationUser);
			unitOfWork.Commit();
			unitOfWork.Dispose();

			return NoContent();
		}
	}
}
