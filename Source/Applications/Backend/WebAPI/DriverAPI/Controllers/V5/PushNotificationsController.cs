using DriverApi.Contracts.V5;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V5.Services;
using DriverAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер PUSH-сообщений
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize]
	public class PushNotificationsController : VersionedController
	{
		private readonly ILogger<PushNotificationsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IRouteListService _routeListService;
		private readonly IFCMAPIHelper _iFCMAPIHelper;
		private readonly IEmployeeService _employeeService;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="routeListService"></param>
		/// <param name="iFCMAPIHelper"></param>
		/// <param name="employeeService"></param>
		/// <param name="wakeUpDriverClientService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PushNotificationsController(
			ILogger<PushNotificationsController> logger,
			UserManager<IdentityUser> userManager,
			IRouteListService routeListService,
			IFCMAPIHelper iFCMAPIHelper,
			IEmployeeService employeeService,
			IWakeUpDriverClientService wakeUpDriverClientService) : base(logger)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_routeListService = routeListService
				?? throw new ArgumentNullException(nameof(routeListService));
			_iFCMAPIHelper = iFCMAPIHelper
				?? throw new ArgumentNullException(nameof(iFCMAPIHelper));
			_employeeService = employeeService
				?? throw new ArgumentNullException(nameof(employeeService));
			_wakeUpDriverClientService = wakeUpDriverClientService
				?? throw new ArgumentNullException(nameof(wakeUpDriverClientService));
		}

		/// <summary>
		/// Подписка на PUSH-уведомления
		/// </summary>
		/// <param name="enablePushNotificationsRequest"></param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> EnablePushNotificationsAsync([FromBody] EnablePushNotificationsRequestDto enablePushNotificationsRequest)
		{
			_logger.LogInformation("Запрошена подписка на PUSH-сообщения для пользователя {Username} Firebase token: {FirebaseToken}, User token: {AccessToken}",
				HttpContext.User.Identity?.Name ?? "Unknown",
				enablePushNotificationsRequest.Token,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);
			_wakeUpDriverClientService.Subscribe(driver, enablePushNotificationsRequest.Token);
			_employeeService.EnablePushNotifications(driver.DriverAppUser, enablePushNotificationsRequest.Token);

			return NoContent();
		}

		/// <summary>
		/// Отписка от PUSH-уведомлений
		/// </summary>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DisablePushNotificationsAsync()
		{
			_logger.LogInformation("Запрошена отписка от PUSH-сообщений для пользователя {Username} User token: {AccessToken}",
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);
			_wakeUpDriverClientService.UnSubscribe(driver);
			_employeeService.DisablePushNotifications(driver.DriverAppUser);

			return NoContent();
		}

		/// <summary>
		/// Уведомление о смене формы оплаты в заказе
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> NotifyOfSmsPaymentStatusChanged([FromBody] int orderId)
		{
			await SendPaymentStatusChangedPushNotificationAsync(orderId);

			return NoContent();
		}

		/// <summary>
		/// Уведомление о смене типа оплаты заказа
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns></returns>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> NotifyOfFastPaymentStatusChanged([FromBody] int orderId)
		{
			await SendPaymentStatusChangedPushNotificationAsync(orderId);
		
			return NoContent();
		}

		/// <summary>
		/// Уведомления о новом поступившем заказе с доставкой за час
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> NotifyOfFastDeliveryOrderAddedAsync([FromBody] int orderId)
		{
			var token = _routeListService.GetActualDriverPushNotificationsTokenByOrderId(orderId);

			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation("Отправка PUSH-сообщения прервана, водитель заказа {OrderId} не подписан на PUSH-сообщения.", orderId);
			}
			else
			{
				_logger.LogInformation("Отправка PUSH-сообщения о добавлении заказа ({OrderId}) для доставки за час", orderId);
				await _iFCMAPIHelper.SendPushNotification(token, "Уведомление о добавлении заказа за час", $"Добавлен заказ {orderId} с доставкой за час");
			}
		
			return NoContent();
		}

		/// <summary>
		/// Уведомления об изменении времени ожидания
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> NotifyOfWaitingTimeChangedAsync([FromBody] int orderId)
		{
			var token = _routeListService.GetActualDriverPushNotificationsTokenByOrderId(orderId);

			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation("Отправка PUSH-сообщения прервана, водитель заказа {OrderId} не подписан на PUSH-сообщения.", orderId);
			}
			else
			{
				_logger.LogInformation("Отправка PUSH-сообщения об изменении времени ожидания заказа ({OrderId})", orderId);
				await _iFCMAPIHelper.SendPushNotification(token, "Уведомление об изменении времени ожидания заказа", $"Время ожидания заказа {orderId} изменено");
			}

			return NoContent();
		}

		private async Task SendPaymentStatusChangedPushNotificationAsync(int orderId)
		{
			var token = _routeListService.GetActualDriverPushNotificationsTokenByOrderId(orderId);

			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation("Отправка PUSH-сообщения прервана, водитель заказа {OrderId} не подписан на PUSH-сообщения.", orderId);
			}
			else
			{
				_logger.LogInformation("Отправка PUSH-сообщения об изменении статуса заказа {OrderId}", orderId);
				await _iFCMAPIHelper.SendPushNotification(token, "Веселый водовоз", $"Обновлен статус платежа для заказа {orderId}");
			}
		}
	}
}
