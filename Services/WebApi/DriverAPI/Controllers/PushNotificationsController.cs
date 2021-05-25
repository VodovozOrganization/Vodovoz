using DriverAPI.Library.DataAccess;
using DriverAPI.Library.Helpers;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class PushNotificationsController : ControllerBase
	{
		private readonly ILogger<PushNotificationsController> logger;
		private readonly UserManager<IdentityUser> userManager;
		private readonly IAPIRouteListData aPIRouteListData;
		private readonly IFCMAPIHelper iFCMAPIHelper;
		private readonly IEmployeeData employeeData;

		public PushNotificationsController(
			ILogger<PushNotificationsController> logger,
			UserManager<IdentityUser> userManager,
			IAPIRouteListData aPIRouteListData,
			IFCMAPIHelper iFCMAPIHelper,
			IEmployeeData employeeData)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			this.aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			this.iFCMAPIHelper = iFCMAPIHelper ?? throw new ArgumentNullException(nameof(iFCMAPIHelper));
			this.employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
		}

		/// <summary>
		/// Эндпоинт включения PUSH уведомлений
		/// </summary>
		/// <param name="enablePushNotificationsRequest"></param>
		[HttpPost]
		[Route("/api/EnablePushNotifications")]
		public void EnablePushNotifications([FromBody] EnablePushNotificationsRequestModel enablePushNotificationsRequest)
		{
			var user = userManager.GetUserAsync(User).Result;
			var driver = employeeData.GetByAPILogin(user.UserName);
			employeeData.EnablePushNotifications(driver, enablePushNotificationsRequest.Token);
		}

		/// <summary>
		/// Эндпоинт отключения PUSH уведомлений
		/// </summary>
		[HttpPost]
		[Route("/api/DisablePushNotifications")]
		public void DisablePushNotifications()
		{
			var user = userManager.GetUserAsync(User).Result;
			var driver = employeeData.GetByAPILogin(user.UserName);
			employeeData.DisablePushNotifications(driver);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="orderId"></param>
		[HttpPost]
		[AllowAnonymous]
		[Route("/api/NotifyOfSmsPaymentStatusChanged")]
		public void NotifyOfSmsPaymentStatusChanged([FromBody] int orderId)
		{
			var token = aPIRouteListData.GetActualDriverPushNotificationsTokenByOrderId(orderId);
			iFCMAPIHelper.SendPushNotification(token, "Веселый водовоз", "Обновлен статус платежа").Wait();
		}
	}
}
