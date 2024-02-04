using DriverAPI.DTOs.V4;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using DriverAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер PUSH-сообщений
	/// </summary>
	[Authorize]
	public class PushNotificationsController : VersionedController
	{
		private readonly ILogger<PushNotificationsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IRouteListModel _aPIRouteListData;
		private readonly IFCMAPIHelper _iFCMAPIHelper;
		private readonly IEmployeeModel _employeeData;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="aPIRouteListData"></param>
		/// <param name="iFCMAPIHelper"></param>
		/// <param name="employeeData"></param>
		/// <param name="wakeUpDriverClientService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PushNotificationsController(
			ILogger<PushNotificationsController> logger,
			UserManager<IdentityUser> userManager,
			IRouteListModel aPIRouteListData,
			IFCMAPIHelper iFCMAPIHelper,
			IEmployeeModel employeeData,
			IWakeUpDriverClientService wakeUpDriverClientService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			_iFCMAPIHelper = iFCMAPIHelper ?? throw new ArgumentNullException(nameof(iFCMAPIHelper));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_wakeUpDriverClientService = wakeUpDriverClientService ?? throw new ArgumentNullException(nameof(wakeUpDriverClientService));
		}

		/// <summary>
		/// Подписка на PUSH-уведомления
		/// </summary>
		/// <param name="enablePushNotificationsRequest"></param>
		[HttpPost]
		[Route("EnablePushNotifications")]
		public async Task EnablePushNotificationsAsync([FromBody] EnablePushNotificationsRequestDto enablePushNotificationsRequest)
		{
			_logger.LogInformation("Запрошена подписка на PUSH-сообщения для пользователя {Username} Firebase token: {FirebaseToken}, User token: {AccessToken}",
				HttpContext.User.Identity?.Name ?? "Unknown",
				enablePushNotificationsRequest.Token,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);
			_wakeUpDriverClientService.Subscribe(driver, enablePushNotificationsRequest.Token);
			_employeeData.EnablePushNotifications(driver.DriverAppUser, enablePushNotificationsRequest.Token);
		}

		/// <summary>
		/// Отписка от PUSH-уведомлений
		/// </summary>
		[HttpPost]
		[Route("DisablePushNotifications")]
		public async Task DisablePushNotificationsAsync()
		{
			_logger.LogInformation("Запрошена отписка от PUSH-сообщений для пользователя {Username} User token: {AccessToken}",
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);
			_wakeUpDriverClientService.UnSubscribe(driver);
			_employeeData.DisablePushNotifications(driver.DriverAppUser);

		}

		/// <summary>
		/// Уведомление о смене формы оплаты в заказе
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		[HttpPost]
		[AllowAnonymous]
		[Route("NotifyOfSmsPaymentStatusChanged")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task NotifyOfSmsPaymentStatusChanged([FromBody] int orderId)
		{
			await SendPaymentStatusChangedPushNotificationAsync(orderId);
		}

		/// <summary>
		/// Уведомление о смене типа оплаты заказа
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns></returns>
		[HttpPost]
		[AllowAnonymous]
		[Route("NotifyOfFastPaymentStatusChanged")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task NotifyOfFastPaymentStatusChanged([FromBody] int orderId)
		{
			await SendPaymentStatusChangedPushNotificationAsync(orderId);
		}

		private async Task SendPaymentStatusChangedPushNotificationAsync(int orderId)
		{
			var token = _aPIRouteListData.GetActualDriverPushNotificationsTokenByOrderId(orderId);
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

		/// <summary>
		/// Уведомления о новом поступившем заказе с доставкой за час
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		[HttpPost]
		[AllowAnonymous]
		[Route("NotifyOfFastDeliveryOrderAdded")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task NotifyOfFastDeliveryOrderAdded([FromBody] int orderId)
		{
			var token = _aPIRouteListData.GetActualDriverPushNotificationsTokenByOrderId(orderId);
			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation("Отправка PUSH-сообщения прервана, водитель заказа {OrderId} не подписан на PUSH-сообщения.", orderId);
			}
			else
			{
				_logger.LogInformation("Отправка PUSH-сообщения о добавлении заказа ({OrderId}) для доставки за час", orderId);
				await _iFCMAPIHelper.SendPushNotification(token, "Уведомление о добавлении заказа за час", $"Добавлен заказ {orderId} с доставкой за час");
			}
		}

		/// <summary>
		/// Уведомления об изменении времени ожидания
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		[HttpPost]
		[AllowAnonymous]
		[Route("NotifyOfWaitingTimeChanged")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task NotifyOfWaitingTimeChanged([FromBody] int orderId)
		{
			var token = _aPIRouteListData.GetActualDriverPushNotificationsTokenByOrderId(orderId);
			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation("Отправка PUSH-сообщения прервана, водитель заказа {OrderId} не подписан на PUSH-сообщения.", orderId);
			}
			else
			{
				_logger.LogInformation("Отправка PUSH-сообщения об изменении времени ожидания заказа ({OrderId})", orderId);
				await _iFCMAPIHelper.SendPushNotification(token, "Уведомление об изменении времени ожидания заказа", $"Время ожидания заказа {orderId} изменено");
			}
		}
	}
}
