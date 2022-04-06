using DriverAPI.Library.Models;
using DriverAPI.Library.Helpers;
using DriverAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class PushNotificationsController : ControllerBase
	{
		private readonly ILogger<PushNotificationsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IRouteListModel _aPIRouteListData;
		private readonly IFCMAPIHelper _iFCMAPIHelper;
		private readonly IEmployeeModel _employeeData;

		public PushNotificationsController(
			ILogger<PushNotificationsController> logger,
			UserManager<IdentityUser> userManager,
			IRouteListModel aPIRouteListData,
			IFCMAPIHelper iFCMAPIHelper,
			IEmployeeModel employeeData)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			_iFCMAPIHelper = iFCMAPIHelper ?? throw new ArgumentNullException(nameof(iFCMAPIHelper));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
		}

		/// <summary>
		/// Эндпоинт включения PUSH уведомлений
		/// </summary>
		/// <param name="enablePushNotificationsRequest"></param>
		[HttpPost]
		[Route("/api/EnablePushNotifications")]
		public void EnablePushNotifications([FromBody] EnablePushNotificationsRequestDto enablePushNotificationsRequest)
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			_logger.LogInformation($"(FirebaseToken: {enablePushNotificationsRequest.Token}) User token: {tokenStr}");

			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);
			_employeeData.EnablePushNotifications(driver, enablePushNotificationsRequest.Token);
		}

		/// <summary>
		/// Эндпоинт отключения PUSH уведомлений
		/// </summary>
		[HttpPost]
		[Route("/api/DisablePushNotifications")]
		public void DisablePushNotifications()
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			_logger.LogInformation($"User token: {tokenStr}");

			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);
			_employeeData.DisablePushNotifications(driver);
		}

		/// <summary>
		/// Эндпоинт уведомления о смене формы оплаты в заказе
		/// </summary>
		/// <param name="orderId">Id заказа</param>
		[HttpPost]
		[AllowAnonymous]
		[Route("/api/NotifyOfSmsPaymentStatusChanged")]
		public async Task NotifyOfSmsPaymentStatusChanged([FromBody] int orderId)
		{
			var token = _aPIRouteListData.GetActualDriverPushNotificationsTokenByOrderId(orderId);
			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation($"No token found for order driver PUSH message. Order: {orderId}");
			}
			else
			{
				_logger.LogInformation($"Sending PUSH message of status changed for order: {orderId}");
				await _iFCMAPIHelper.SendPushNotification(token, "Веселый водовоз", $"Обновлен статус платежа для заказа {orderId}");
			}
		}

		/// <summary>
		/// Эндпоинт уведомления о новом поступившем заказе с быстрой доставкой
		/// </summary>
		/// <param name="orderId">Id заказа</param>
		[HttpPost]
		[AllowAnonymous]
		[Route("/api/NotifyOfFastDeliveryOrderAdded")]
		public async Task NotifyOfFastDeliveryOrderAdded([FromBody] int orderId)
		{
			var token = _aPIRouteListData.GetActualDriverPushNotificationsTokenByOrderId(orderId);
			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation($"No token found for order driver PUSH message. Order: {orderId}");
			}
			else
			{
				_logger.LogInformation($"Sending PUSH message of fast delivery order ({orderId}) added");
				await _iFCMAPIHelper.SendPushNotification(token, "Уведомление о добавлении заказа за час", $"Добавлен заказ { orderId } с доставкой за час");
			}
		}
	}
}
