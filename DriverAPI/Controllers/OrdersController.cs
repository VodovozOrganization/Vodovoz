using DriverAPI.Library.DataAccess;
using DriverAPI.Library.Models;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class OrdersController : ControllerBase
	{
		private readonly IEmployeeData employeeData;
		private readonly UserManager<IdentityUser> userManager;
		private readonly IAPIOrderData aPIOrderData;

		public OrdersController(
			IEmployeeData employeeData,
			UserManager<IdentityUser> userManager,
			IAPIOrderData aPIOrderData)
		{
			this.employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			this.aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
		}

		/// <summary>
		/// Эндпоинт получения информации о заказе
		/// В ответе сервера будет JSON объект с полями соответствующими APIOrder
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		[HttpGet]
		[Route("/api/GetOrder")]
		public APIOrder Get(int orderId)
		{
			return aPIOrderData.Get(orderId);
		}

		// POST: CompleteOrderDelivery / CompleteRouteListAddress
		[HttpPost]
		[Route("/api/CompleteOrderDelivery")]
		public void CompleteOrderDelivery([FromBody] CompletedOrderRequestModel completedOrderRequestModel)
		{
			var user = userManager.GetUserAsync(User).Result;
			var driver = employeeData.GetByAPILogin(user.UserName);

			aPIOrderData.CompleteOrderDelivery(
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
		public void ChangeOrderPaymentType(ChangeOrderPaymentTypeRequestModel changeOrderPaymentTypeRequestModel)
		{
			var orderId = changeOrderPaymentTypeRequestModel.OrderId;
			var newPaymentType = changeOrderPaymentTypeRequestModel.NewPaymentType;

			IEnumerable<APIPaymentType> availableTypesToChange = aPIOrderData.GetAvailableToChangePaymentTypes(orderId);

			if (!availableTypesToChange.Contains(newPaymentType))
			{
				throw new ArgumentOutOfRangeException($"Попытка сменить тип оплаты у заказа {orderId} на недоступный для этого заказа тип оплаты {newPaymentType}");
			}

			Vodovoz.Domain.Client.PaymentType newVodovozPaymentType;

			if (newPaymentType == APIPaymentType.Terminal)
			{
				newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Terminal;
			}
			else if (newPaymentType == APIPaymentType.Cash)
			{
				newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.cash;
			}
			else
			{
				throw new ArgumentOutOfRangeException($"Попытка сменить тип оплаты у заказа {orderId} на не поддерживаемый для смены тип оплаты {newPaymentType}");
			}

			aPIOrderData.ChangeOrderPaymentType(orderId, newVodovozPaymentType);
		}
	}
}
