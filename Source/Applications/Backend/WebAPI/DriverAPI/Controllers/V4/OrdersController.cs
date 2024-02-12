using DriverAPI.DTOs.V4;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер заказов
	/// </summary>
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class OrdersController : VersionedController
	{
		private readonly ILogger<OrdersController> _logger;
		private readonly IEmployeeModel _employeeData;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IOrderModel _aPIOrderData;
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordModel;
		private readonly IActionTimeHelper _actionTimeHelper;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="employeeData"></param>
		/// <param name="userManager"></param>
		/// <param name="aPIOrderData"></param>
		/// <param name="driverMobileAppActionRecordModel"></param>
		/// <param name="actionTimeHelper"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public OrdersController(
			ILogger<OrdersController> logger,
			IEmployeeModel employeeData,
			UserManager<IdentityUser> userManager,
			IOrderModel aPIOrderData,
			IDriverMobileAppActionRecordModel driverMobileAppActionRecordModel,
			IActionTimeHelper actionTimeHelper)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
			_driverMobileAppActionRecordModel = driverMobileAppActionRecordModel ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordModel));
			_actionTimeHelper = actionTimeHelper ?? throw new ArgumentNullException(nameof(actionTimeHelper));
		}

		/// <summary>
		/// Получение информации о заказе
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		[HttpGet("GetOrder")]
		[Produces("application/json")]
		public OrderDto Get(int orderId)
		{
			_logger.LogInformation("(OrderId: {OrderId}) User token: {AccessToken}",
				orderId,
				Request.Headers[HeaderNames.Authorization]);

			return _aPIOrderData.Get(orderId);
		}

		/// <summary>
		/// Завершение доставки заказа
		/// </summary>
		/// <param name="completedOrderRequestModel"><see cref="CompletedOrderRequestDto"/></param>
		/// <returns></returns>
		[HttpPost("CompleteOrderDelivery")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task CompleteOrderDeliveryAsync([FromBody] CompletedOrderRequestDto completedOrderRequestModel)
		{
			_logger.LogInformation("(Завершение заказа: {OrderId}) пользователем {Username} | User token: {AccessToken}",
				completedOrderRequestModel.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			var localActionTime = completedOrderRequestModel.ActionTimeUtc.ToLocalTime();

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, localActionTime);

				_aPIOrderData.CompleteOrderDelivery(
					recievedTime,
					driver,
					completedOrderRequestModel,
					completedOrderRequestModel);
			}
			catch(Exception ex)
			{
				resultMessage = ex.Message;
				throw;
			}
			finally
			{
				_driverMobileAppActionRecordModel.RegisterAction(driver, DriverMobileAppActionType.CompleteOrderClicked, localActionTime, recievedTime, resultMessage);
			}
		}

		/// <summary>
		/// Создание рекламации по координатам точки доставки заказа
		/// </summary>
		/// <param name="completedOrderRequestModel"></param>
		/// <returns></returns>
		[HttpPost("UpdateOrderShipmentInfo")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> UpdateOrderShipmentInfoAsync([FromBody] UpdateOrderShipmentInfoRequestDto completedOrderRequestModel)
		{
			_logger.LogInformation("(Создание рекламации по координатам точки доставки заказа: {OrderId}) пользователем {Username} | User token: {AccessToken}",
				completedOrderRequestModel.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			var localActionTime = completedOrderRequestModel.ActionTimeUtc.ToLocalTime();

			_actionTimeHelper.ThrowIfNotValid(recievedTime, localActionTime);

			_aPIOrderData.UpdateOrderShipmentInfo(
				recievedTime,
				driver,
				completedOrderRequestModel);

			return NoContent();
		}

		/// <summary>
		/// Смены типа оплаты заказа
		/// </summary>
		/// <param name="changeOrderPaymentTypeRequestModel">Модель данных входящего запроса</param>
		[HttpPost]
		[Route("ChangeOrderPaymentType")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task ChangeOrderPaymentTypeAsync(ChangeOrderPaymentTypeRequestDto changeOrderPaymentTypeRequestModel)
		{
			var recievedTime = DateTime.Now;

			var orderId = changeOrderPaymentTypeRequestModel.OrderId;
			var newPaymentType = changeOrderPaymentTypeRequestModel.NewPaymentType;

			var localActionTime = changeOrderPaymentTypeRequestModel.ActionTimeUtc.ToLocalTime();

			_logger.LogInformation("Смена типа оплаты заказа: {OrderId} на {PaymentType}" +
				" на стороне мобильного приложения в {ActionTime} пользователем {Username} в {RecievedTime} | User token: {AccessToken}",
				orderId,
				newPaymentType,
				localActionTime,
				HttpContext.User.Identity?.Name ?? "Unknown",
				recievedTime,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, localActionTime);

				IEnumerable<PaymentDtoType> availableTypesToChange = _aPIOrderData.GetAvailableToChangePaymentTypes(orderId);

				if(!availableTypesToChange.Contains(newPaymentType))
				{
					_logger.LogWarning("Попытка сменить тип оплаты у заказа {OrderId} на недоступный для этого заказа тип оплаты {PaymentType}", orderId, newPaymentType);
					throw new ArgumentOutOfRangeException(
						nameof(changeOrderPaymentTypeRequestModel.NewPaymentType),
						$"Попытка сменить тип оплаты у заказа {orderId} на недоступный для этого заказа тип оплаты {newPaymentType}");
				}

				Vodovoz.Domain.Client.PaymentType newVodovozPaymentType;
				Vodovoz.Domain.Client.PaymentByTerminalSource? paymentByTerminalSource = null; ;

				if(newPaymentType == PaymentDtoType.TerminalCard)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Terminal;
					paymentByTerminalSource = Vodovoz.Domain.Client.PaymentByTerminalSource.ByCard;
				}
				else if(newPaymentType == PaymentDtoType.TerminalQR)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Terminal;
					paymentByTerminalSource = Vodovoz.Domain.Client.PaymentByTerminalSource.ByQR;
				}
				else if(newPaymentType == PaymentDtoType.Cash)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Cash;
				}
				else if(newPaymentType == PaymentDtoType.DriverApplicationQR)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.DriverApplicationQR;
				}
				else
				{
					_logger.LogWarning("Попытка сменить тип оплаты у заказа {OrderId} на не поддерживаемый для смены тип оплаты {PaymentType}", orderId, newPaymentType);

					throw new ArgumentOutOfRangeException(
						nameof(changeOrderPaymentTypeRequestModel.NewPaymentType),
						$"Попытка сменить тип оплаты у заказа {orderId} на не поддерживаемый для смены тип оплаты {newPaymentType}");
				}

				_aPIOrderData.ChangeOrderPaymentType(orderId, newVodovozPaymentType, driver, paymentByTerminalSource);
			}
			catch(Exception ex)
			{
				resultMessage = ex.Message;
				throw;
			}
			finally
			{
				_driverMobileAppActionRecordModel.RegisterAction(driver, DriverMobileAppActionType.ChangeOrderPaymentTypeClicked, localActionTime, recievedTime, resultMessage);
			}
		}
	}
}
