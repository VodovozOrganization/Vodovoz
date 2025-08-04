using DriverApi.Contracts.V6.Requests;
using DriverAPI.Library.V6.Services;
using DriverAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DriverApi.Contracts.V6;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using ApiRouteListService = DriverAPI.Library.V6.Services.IRouteListService;
using IRouteListService = Vodovoz.Services.Logistics.IRouteListService;

namespace DriverAPI.Controllers.V6
{
	/// <summary>
	/// Контроллер PUSH-сообщений
	/// </summary>
	[ApiVersion("6.0")]
	[Authorize]
	public class PushNotificationsController : VersionedController
	{
		private readonly ILogger<PushNotificationsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ApiRouteListService _apiRouteListService;
		private readonly IRouteListService _routeListService;
		private readonly IEmployeeService _employeeService;
		private readonly IWakeUpDriverClientService _wakeUpDriverClientService;
		private readonly IFirebaseCloudMessagingService _firebaseCloudMessagingService;
		private readonly IGenericRepository<CashRequest> _cashRequestRepository;
		private readonly IGenericRepository<RouteListItem> _routeListItemRepository;
		private const string _eventTypeName = "EventType";
		private const string _routeListIdName = "RouteListId";


		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="apiRouteListService"></param>
		/// <param name="employeeService"></param>
		/// <param name="wakeUpDriverClientService"></param>
		/// <param name="firebaseCloudMessagingService"></param>
		/// <param name="cashRequestRepository"></param>
		/// <param name="routeListItemRepository"></param>
		/// <param name="routeListService"></param>
		/// <param name="addressTransferDocumentItemRepository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PushNotificationsController(
			ILogger<PushNotificationsController> logger,
			UserManager<IdentityUser> userManager,
			ApiRouteListService apiRouteListService,
			IEmployeeService employeeService,
			IWakeUpDriverClientService wakeUpDriverClientService,
			IFirebaseCloudMessagingService firebaseCloudMessagingService,
			IGenericRepository<CashRequest> cashRequestRepository,
			IGenericRepository<RouteListItem> routeListItemRepository,
			IRouteListService routeListService,
			IGenericRepository<AddressTransferDocumentItem> addressTransferDocumentItemRepository) : base(logger)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_apiRouteListService = apiRouteListService
				?? throw new ArgumentNullException(nameof(apiRouteListService));
			_employeeService = employeeService
				?? throw new ArgumentNullException(nameof(employeeService));
			_wakeUpDriverClientService = wakeUpDriverClientService
				?? throw new ArgumentNullException(nameof(wakeUpDriverClientService));
			_firebaseCloudMessagingService = firebaseCloudMessagingService
				?? throw new ArgumentNullException(nameof(firebaseCloudMessagingService));
			_cashRequestRepository = cashRequestRepository
				?? throw new ArgumentNullException(nameof(cashRequestRepository));
			_routeListItemRepository = routeListItemRepository
				?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListService = routeListService
				?? throw new ArgumentNullException(nameof(routeListService));
		}

		/// <summary>
		/// Подписка на PUSH-уведомления
		/// </summary>
		/// <param name="enablePushNotificationsRequest"></param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> EnablePushNotificationsAsync([FromBody] EnablePushNotificationsRequest enablePushNotificationsRequest)
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
			var token = _apiRouteListService.GetActualDriverPushNotificationsTokenByOrderId(orderId);

			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation("Отправка PUSH-сообщения прервана, водитель заказа {OrderId} не подписан на PUSH-сообщения.", orderId);
			}
			else
			{
				_logger.LogInformation("Отправка PUSH-сообщения о добавлении заказа ({OrderId}) для доставки за час", orderId);
				await _firebaseCloudMessagingService.SendMessage(token, "Уведомление о добавлении заказа за час", $"Добавлен заказ {orderId} с доставкой за час");
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
			var token = _apiRouteListService.GetActualDriverPushNotificationsTokenByOrderId(orderId);

			if(string.IsNullOrWhiteSpace(token))
			{
				_logger.LogInformation("Отправка PUSH-сообщения прервана, водитель заказа {OrderId} не подписан на PUSH-сообщения.", orderId);
			}
			else
			{
				_logger.LogInformation("Отправка PUSH-сообщения об изменении времени ожидания заказа ({OrderId})", orderId);
				await _firebaseCloudMessagingService.SendMessage(token, "Уведомление об изменении времени ожидания заказа", $"Время ожидания заказа {orderId} изменено");
			}

			return NoContent();
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
		public async Task<IActionResult> NotifyOfCashRequestForDriverIsGivenForTake([FromBody] int cashRequestId, [FromServices] IUnitOfWork unitOfWork)
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

				return Problem($"Заявка на выдачу денежных средств {cashRequestId} не найдена");
			}

			var driversIds = cashRequest.Sums
				.Select(sum => sum.AccountableEmployee)
				.Where(e => e.ExternalApplicationsUsers.Any(eau => eau.ExternalApplicationType == Vodovoz.Core.Domain.Employees.ExternalApplicationType.DriverApp))
				.Select(d => d.Id);

			var sendedToDrivers = new List<int>();

			foreach(var notifyableDriverId in driversIds)
			{
				var firebaseToken = _employeeService.GetDriverPushTokenById(notifyableDriverId);

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

		private async Task SendPaymentStatusChangedPushNotificationAsync(int orderId)
		{
			var token = _apiRouteListService.GetActualDriverPushNotificationsTokenByOrderId(orderId);

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
		/// Оповещение об изменениях в МЛ
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="notificationRouteListChangesRequest">Уведомление</param>
		/// <returns></returns>
		[HttpPost]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> NotifyOfRouteListChanged(
			[FromServices] IUnitOfWork unitOfWork,
			[FromBody] NotificationRouteListChangesRequest notificationRouteListChangesRequest)
		{
			var orderId = notificationRouteListChangesRequest?.OrderId;

			switch(notificationRouteListChangesRequest.PushNotificationDataEventType)
			{
				case PushNotificationDataEventType.TransferAddressFromHandToHand:
					return await NotifyOfOrderTransferFromHandToHand(unitOfWork, orderId.Value);
				case PushNotificationDataEventType.RouteListContentChanged:
					return await NotifyOfRouteListContentChangedAsync(unitOfWork, orderId.Value);
			}

			return Problem($"Неизвестный запрос на уведомление водителя по заказу {orderId.Value}",
				statusCode: StatusCodes.Status422UnprocessableEntity);
		}
		
		private async Task<IActionResult> NotifyOfOrderTransferFromHandToHand(
			IUnitOfWork unitOfWork,
			int orderId)
		{
			var targetAddress = _routeListItemRepository
				.Get(
					unitOfWork,
					rli => rli.Status == RouteListItemStatus.EnRoute
						&& rli.AddressTransferType == AddressTransferType.FromHandToHand
						&& rli.Order.Id == orderId)
				.FirstOrDefault();

			if(targetAddress is null)
			{
				_logger.LogError("Не найдена цель переноса заказа {OrderId}", orderId);

				return Problem($"Не найдена цель переноса заказа {orderId}", statusCode: StatusCodes.Status400BadRequest);
			}

			if(targetAddress.RouteList.Driver is null)
			{
				_logger.LogError("Не найден водитель цели переноса заказа {OrderId}", orderId);

				return Problem($"Не найден водитель цели переноса заказа {orderId}", statusCode: StatusCodes.Status400BadRequest);
			}

			var targetDriverFirebaseToken = _employeeService.GetDriverPushTokenById(targetAddress.RouteList.Driver.Id);

			var source = _routeListService.FindTransferSource(unitOfWork, targetAddress);

			var previousItemResult = _routeListService.FindPrevious(unitOfWork, targetAddress);

			if(previousItemResult.IsFailure)
			{
				_logger.LogError("Не найден предыдущий адрес МЛ заказа {OrderId}", orderId);

				return Problem($"Не найден предыдущий адрес МЛ заказа {orderId}", statusCode: StatusCodes.Status400BadRequest);
			}

			var previousItemDriverFirebaseToken = _employeeService.GetDriverPushTokenById(previousItemResult.Value.RouteList.Driver.Id);

			var message = string.Empty;

			var previousItemDriverFirebaseTokenFound = !string.IsNullOrWhiteSpace(previousItemDriverFirebaseToken);
			var targetDriverFirebaseTokenFound = !string.IsNullOrWhiteSpace(targetDriverFirebaseToken);

			if(!previousItemDriverFirebaseTokenFound)
			{
				message += $"Водитель из МЛ которого переносится заказ не будет оповещен.\n";
			}

			if(!targetDriverFirebaseTokenFound)
			{
				message += $"Водитель которому переносится заказ не будет оповещен.\n";
			}
			
			var previousData = new Dictionary<string, string>
			{
				{ _eventTypeName, PushNotificationDataEventType.TransferAddressFromHandToHand.ToString() },
				{ _routeListIdName, previousItemResult.Value.RouteList.Id.ToString() }
			};
			
			var targetData = new Dictionary<string, string>
			{
				{ _eventTypeName, PushNotificationDataEventType.TransferAddressFromHandToHand.ToString() },
				{ _routeListIdName, targetAddress.RouteList.Id.ToString() }
			};

			if(source.IsFailure)
			{
				if(previousItemDriverFirebaseTokenFound)
				{
					await _firebaseCloudMessagingService.SendMessage(previousItemDriverFirebaseToken, "Веселый водовоз", $"Перенос заказа №{orderId} отменен", previousData);
				}

				if(targetDriverFirebaseTokenFound)
				{
					await _firebaseCloudMessagingService.SendMessage(targetDriverFirebaseToken, "Веселый водовоз", $"Перенос заказа №{orderId} отменен", targetData);
				}

				if(previousItemDriverFirebaseTokenFound && targetDriverFirebaseTokenFound)
				{
					return NoContent();
				}
				else
				{
					return Problem(message.Trim('\n'), statusCode: StatusCodes.Status202Accepted);
				}
			}
			
			var sourceData = new Dictionary<string, string>
			{
				{ _eventTypeName, PushNotificationDataEventType.TransferAddressFromHandToHand.ToString() },
				{ _routeListIdName,  source.Value.RouteList.Id.ToString() }
			};

			if(previousItemResult.Value.RouteList.Id != source.Value.RouteList.Id)
			{
				if(previousItemDriverFirebaseTokenFound)
				{
					await _firebaseCloudMessagingService.SendMessage(previousItemDriverFirebaseToken, "Веселый водовоз", $"Перенос заказа №{orderId} отменен", previousData);
				}
			}

			var sourceDriverFirebaseToken = _employeeService.GetDriverPushTokenById(source.Value.RouteList.Driver.Id);

			var sourceDriverFirebaseTokenFound = !string.IsNullOrWhiteSpace(sourceDriverFirebaseToken);

			if(sourceDriverFirebaseTokenFound)
			{
				await _firebaseCloudMessagingService.SendMessage(sourceDriverFirebaseToken, "Веселый водовоз", $"Заказ №{orderId} необходимо передать другому водителю", sourceData);
			}
			else
			{
				message += $"Водитель источника переноса заказа не будет оповещен.\n";
			}

			if(targetDriverFirebaseTokenFound)
			{
				await _firebaseCloudMessagingService.SendMessage(targetDriverFirebaseToken, "Веселый водовоз", $"Вам передан заказ №{orderId}", targetData);
			}

			if(message == string.Empty)
			{
				return NoContent();
			}
			else
			{
				return Problem(message.Trim('\n'), statusCode: StatusCodes.Status202Accepted);
			}
		}

		private async Task<IActionResult> NotifyOfRouteListContentChangedAsync(IUnitOfWork unitOfWork, int orderId)
		{
			var token = _apiRouteListService.GetActualDriverPushNotificationsTokenByOrderId(orderId);

			if(string.IsNullOrWhiteSpace(token))
			{
				var message = $"Отправка PUSH-сообщения прервана, водитель заказа {orderId} не подписан на PUSH-сообщения.";

				return Problem(message, statusCode: StatusCodes.Status404NotFound);
			}

			var routeListId = _routeListItemRepository
				.Get(
					unitOfWork,
					rli => rli.Order.Id == orderId && rli.Status != RouteListItemStatus.Transfered)
				.Select(rli => rli.RouteList.Id)
				.SingleOrDefault();

			if(routeListId == 0)
			{
				var message = $"Отправка PUSH-сообщения прервана, МЛ для заказа {orderId} не найден.";

				return Problem(message, statusCode: StatusCodes.Status404NotFound);
			}

			_logger.LogInformation("Отправка PUSH-сообщения об изменении состава маршрутного листа ({OrderId})", orderId);

			var data = new Dictionary<string, string>
			{
				{ _eventTypeName, PushNotificationDataEventType.RouteListContentChanged.ToString() },
				{ _routeListIdName, routeListId.ToString() }
			};

			await _firebaseCloudMessagingService.SendMessage(token, $"Уведомление об изменении состава вашего МЛ {routeListId}",
				"Состав вашего маршрутного листа был изменен", data);

			return NoContent();
		}
	}
}
