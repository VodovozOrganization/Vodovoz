using DriverAPI.Library.Models;
using DriverAPI.Library.DTOs;
using DriverAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class OrdersController : ControllerBase
	{
		private readonly ILogger<OrdersController> _logger;
		private readonly IEmployeeModel _employeeData;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IOrderModel _aPIOrderData;

		public OrdersController(
			ILogger<OrdersController> logger,
			IEmployeeModel employeeData,
			UserManager<IdentityUser> userManager,
			IOrderModel aPIOrderData)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
		}

		/// <summary>
		/// Эндпоинт получения информации о заказе
		/// В ответе сервера будет JSON объект с полями соответствующими APIOrder
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		[HttpGet]
		[Route("/api/GetOrder")]
		public OrderDto Get(int orderId)
		{
			return _aPIOrderData.Get(orderId);
		}

		// POST: CompleteOrderDelivery / CompleteRouteListAddress
		[HttpPost]
		[Route("/api/CompleteOrderDelivery")]
		public void CompleteOrderDelivery([FromBody] CompletedOrderRequestDto completedOrderRequestModel)
		{
			_logger.LogInformation($"Завершение заказа: { completedOrderRequestModel.OrderId } пользователем {HttpContext.User.Identity?.Name ?? "Unknown"}");

			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);

			_aPIOrderData.CompleteOrderDelivery(
				driver,
				completedOrderRequestModel.OrderId,
				completedOrderRequestModel.BottlesReturnCount,
				completedOrderRequestModel.Rating,
				completedOrderRequestModel.DriverComplaintReasonId,
				completedOrderRequestModel.OtherDriverComplaintReasonComment,
				completedOrderRequestModel.ActionTime
			);
		}

		/// <summary>
		/// Эндпоинт смены типа оплаты заказа
		/// </summary>
		/// <param name="changeOrderPaymentTypeRequestModel">Модель данных входящего запроса</param>
		[HttpPost]
		[Route("/api/ChangeOrderPaymentType")]
		public void ChangeOrderPaymentType(ChangeOrderPaymentTypeRequestDto changeOrderPaymentTypeRequestModel)
		{
			var orderId = changeOrderPaymentTypeRequestModel.OrderId;
			var newPaymentType = changeOrderPaymentTypeRequestModel.NewPaymentType;

			_logger.LogInformation($"Смена типа оплаты заказа: { orderId } на { newPaymentType } на стороне приложения в { changeOrderPaymentTypeRequestModel.ActionTime } пользователем {HttpContext.User.Identity?.Name ?? "Unknown"}");

			IEnumerable<PaymentDtoType> availableTypesToChange = _aPIOrderData.GetAvailableToChangePaymentTypes(orderId);

			if (!availableTypesToChange.Contains(newPaymentType))
			{
				var errorMessage = $"Попытка сменить тип оплаты у заказа { orderId } на недоступный для этого заказа тип оплаты { newPaymentType }";
				_logger.LogWarning(errorMessage);
				throw new ArgumentOutOfRangeException(nameof(changeOrderPaymentTypeRequestModel.NewPaymentType), errorMessage);
			}

			Vodovoz.Domain.Client.PaymentType newVodovozPaymentType;

			if (newPaymentType == PaymentDtoType.Terminal)
			{
				newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Terminal;
			}
			else if (newPaymentType == PaymentDtoType.Cash)
			{
				newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.cash;
			}
			else
			{
				var errorMessage = $"Попытка сменить тип оплаты у заказа { orderId } на не поддерживаемый для смены тип оплаты { newPaymentType }";
				_logger.LogWarning(errorMessage);
				throw new ArgumentOutOfRangeException(nameof(changeOrderPaymentTypeRequestModel.NewPaymentType), errorMessage);
			}

			_aPIOrderData.ChangeOrderPaymentType(orderId, newVodovozPaymentType);
		}
	}
}
