using DriverApi.Contracts.V4;
using DriverAPI.Library.V4.Models;
using DriverAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using QS.DomainModel.UoW;
using System;
using System.Net.Mime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Cash;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер PUSH-сообщений
	/// </summary>
	[ApiVersion("4.0", Deprecated = true)]
	[Authorize]
	public class PushNotificationsController : VersionedController
	{
		private readonly ILogger<PushNotificationsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IRouteListModel _aPIRouteListData;
		private readonly IEmployeeModel _employeeData;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;
		private readonly IFirebaseCloudMessagingService _firebaseCloudMessagingService;
		private readonly IGenericRepository<CashRequest> _cashRequestRepository;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="aPIRouteListData"></param>
		/// <param name="employeeData"></param>
		/// <param name="wakeUpDriverClientService"></param>
		/// <param name="firebaseCloudMessagingService"></param>
		/// <param name="cashRequestRepository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PushNotificationsController(
			ILogger<PushNotificationsController> logger,
			UserManager<IdentityUser> userManager,
			IRouteListModel aPIRouteListData,
			IEmployeeModel employeeData,
			IWakeUpDriverClientService wakeUpDriverClientService,
			IFirebaseCloudMessagingService firebaseCloudMessagingService,
			IGenericRepository<CashRequest> cashRequestRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_wakeUpDriverClientService = wakeUpDriverClientService ?? throw new ArgumentNullException(nameof(wakeUpDriverClientService));
			_firebaseCloudMessagingService = firebaseCloudMessagingService ?? throw new ArgumentNullException(nameof(firebaseCloudMessagingService));
			_cashRequestRepository = cashRequestRepository ?? throw new ArgumentNullException(nameof(cashRequestRepository));
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
				await _firebaseCloudMessagingService.SendMessage(token, "Веселый водовоз", $"Обновлен статус платежа для заказа {orderId}");
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
				await _firebaseCloudMessagingService.SendMessage(token, "Уведомление о добавлении заказа за час", $"Добавлен заказ {orderId} с доставкой за час");
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
				await _firebaseCloudMessagingService.SendMessage(token, "Уведомление об изменении времени ожидания заказа", $"Время ожидания заказа {orderId} изменено");
			}
		}

		/// <summary>
		/// Уведомление о смене статуса заявки на выдачу ДС на "Передана на выдачу"
		/// Оповещает о премии водителей
		/// </summary>
		/// <param name="cashRequestId"></param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> NotifyOfCashRequestForDriverIsGivenForTake(int cashRequestId, [FromServices] IUnitOfWork unitOfWork)
		{
			var cashRequest = _cashRequestRepository
				.Get(
					unitOfWork,
					cr => cr.Id == cashRequestId && cr.PayoutRequestState == PayoutRequestState.GivenForTake)
				.FirstOrDefault();

			if(cashRequest is null)
			{
				_logger.LogWarning(
					"Не найдена заявка на выдачу денежных средств {CashRequestId} или заявка не в статусе {PayoutRequestState}",
					cashRequestId,
					PayoutRequestState.GivenForTake);
				return BadRequest();
			}

			var driversIds = cashRequest.Sums
				.Select(sum => sum.AccountableEmployee)
				.Where(e => e.ExternalApplicationsUsers.Any(eau => eau.ExternalApplicationType == Vodovoz.Core.Domain.Employees.ExternalApplicationType.DriverApp))
				.Select(d => d.Id);

			var sendedToDrivers = new List<int>();

			foreach(var notifyableDriverId in driversIds)
			{
				var firebaseToken = _employeeData.GetDriverPushTokenById(notifyableDriverId);

				if(string.IsNullOrWhiteSpace(firebaseToken))
				{
					_logger.LogInformation(
						"Отправка PUSH-сообщения о премии прервана, не найден водитель {DriverId} или у него отсутствует токен для PUSH-сообщений",
						notifyableDriverId);

					continue;
				}

				try
				{
					await _firebaseCloudMessagingService.SendMessage(
						firebaseToken,
						"Веселый водовоз",
						"Вам начислена премия, просьба пройти в кассу для ее получения");

					sendedToDrivers.Add(notifyableDriverId);

					_logger.LogInformation(
						"PUSH-сообщения о переведении заявки на выдачу денежных средств {CashRequestId} сотруднику {DriverId} отправлено",
						cashRequestId,
						notifyableDriverId);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "PUSH-сообщения о переведении заявки на выдачу денежных средств {CashRequestId} сотруднику {DriverId} не было отправлено, " +
						"произошла ошибка при отправке PUSH-сообщения: {ExceptionMessage}",
						cashRequestId,
						notifyableDriverId,
						ex.Message);
				}
			}

			_logger.LogInformation(
				"Сообщения о переходе заявки на выдачу денежных средств {CashRequestId} в статус {PayoutRequestState} переданы сотрудникам {@SendedToDriverIds}, сотрудники {@NotSendedToDriverIds} не были оповещены",
				cashRequestId,
				PayoutRequestState.GivenForTake,
				sendedToDrivers,
				driversIds.Except(sendedToDrivers));

			return NoContent();
		}
	}
}
