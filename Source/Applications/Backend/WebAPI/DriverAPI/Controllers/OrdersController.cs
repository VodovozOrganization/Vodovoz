﻿using DriverAPI.DTOs;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Net.Http.Headers;
using Vodovoz.Domain.Logistic.Drivers;
using System.Threading.Tasks;

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
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordModel;
		private readonly IActionTimeHelper _actionTimeHelper;

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
		/// Эндпоинт получения информации о заказе
		/// В ответе сервера будет JSON объект с полями соответствующими APIOrder
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		[HttpGet]
		[Route("/api/GetOrder")]
		public OrderDto Get(int orderId)
		{
			_logger.LogInformation("(OrderId: {OrderId}) User token: {AccessToken}",
				orderId,
				Request.Headers[HeaderNames.Authorization]);

			return _aPIOrderData.Get(orderId);
		}

		// POST: CompleteOrderDelivery / CompleteRouteListAddress
		[HttpPost]
		[Route("/api/CompleteOrderDelivery")]
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

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, completedOrderRequestModel.ActionTime);

				_aPIOrderData.CompleteOrderDelivery(
					driver,
					completedOrderRequestModel.OrderId,
					completedOrderRequestModel.BottlesReturnCount,
					completedOrderRequestModel.Rating,
					completedOrderRequestModel.DriverComplaintReasonId,
					completedOrderRequestModel.OtherDriverComplaintReasonComment,
					completedOrderRequestModel.DriverComment,
					recievedTime
				);
			}
			catch(Exception ex)
			{
				resultMessage = ex.Message;
				throw;
			}
			finally
			{
				_driverMobileAppActionRecordModel.RegisterAction(driver, DriverMobileAppActionType.CompleteOrderClicked, completedOrderRequestModel.ActionTime, recievedTime, resultMessage);
			}
		}

		/// <summary>
		/// Эндпоинт смены типа оплаты заказа
		/// </summary>
		/// <param name="changeOrderPaymentTypeRequestModel">Модель данных входящего запроса</param>
		[HttpPost]
		[Route("/api/ChangeOrderPaymentType")]
		public async Task ChangeOrderPaymentTypeAsync(ChangeOrderPaymentTypeRequestDto changeOrderPaymentTypeRequestModel)
		{
			var recievedTime = DateTime.Now;

			var orderId = changeOrderPaymentTypeRequestModel.OrderId;
			var newPaymentType = changeOrderPaymentTypeRequestModel.NewPaymentType;

			_logger.LogInformation("Смена типа оплаты заказа: {OrderId} на {PaymentType}" +
				" на стороне мобильного приложения в {ActionTime} пользователем {Username} в {RecievedTime} | User token: {AccessToken}",
				orderId,
				newPaymentType,
				changeOrderPaymentTypeRequestModel.ActionTime,
				HttpContext.User.Identity?.Name ?? "Unknown",
				recievedTime,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, changeOrderPaymentTypeRequestModel.ActionTime);

				IEnumerable<PaymentDtoType> availableTypesToChange = _aPIOrderData.GetAvailableToChangePaymentTypes(orderId);

				if(!availableTypesToChange.Contains(newPaymentType))
				{
					var errorFormat = "Попытка сменить тип оплаты у заказа {OrderId} на недоступный для этого заказа тип оплаты {PaymentType}";
					_logger.LogWarning(errorFormat, orderId, newPaymentType);
					throw new ArgumentOutOfRangeException(nameof(changeOrderPaymentTypeRequestModel.NewPaymentType),
						string.Format(errorFormat, orderId, newPaymentType));
				}

				Vodovoz.Domain.Client.PaymentType newVodovozPaymentType;

				if(newPaymentType == PaymentDtoType.Terminal)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Terminal;
				}
				else if(newPaymentType == PaymentDtoType.Cash)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.cash;
				}
				else
				{
					var errorFormat = "Попытка сменить тип оплаты у заказа {OrderId} на не поддерживаемый для смены тип оплаты {PaymentType}";
					_logger.LogWarning(errorFormat, orderId, newPaymentType);
					throw new ArgumentOutOfRangeException(nameof(changeOrderPaymentTypeRequestModel.NewPaymentType),
						string.Format(errorFormat, orderId, newPaymentType));
				}

				_aPIOrderData.ChangeOrderPaymentType(orderId, newVodovozPaymentType, driver);
			}
			catch(Exception ex)
			{
				resultMessage = ex.Message;
				throw;
			}
			finally
			{
				_driverMobileAppActionRecordModel.RegisterAction(driver, DriverMobileAppActionType.ChangeOrderPaymentTypeClicked, changeOrderPaymentTypeRequestModel.ActionTime, recievedTime, resultMessage);
			}
		}
	}
}
