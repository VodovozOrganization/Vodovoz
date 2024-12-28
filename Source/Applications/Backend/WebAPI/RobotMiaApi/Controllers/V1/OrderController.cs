using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NHibernate.Util;
using QS.DomainModel.UoW;
using RobotMiaApi.Contracts.Requests.V1;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Extensions.Mapping;
using RobotMiaApi.Services;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Presentation.WebApi.Common;
using VodovozBusiness.Services.Orders;
using CreateOrderRequest = RobotMiaApi.Contracts.Requests.V1.CreateOrderRequest;

namespace RobotMiaApi.Controllers.V1
{
	/// <summary>
	/// Контроллер заказов
	/// </summary>
	public class OrderController : VersionedController
	{
		private readonly IOrderService _orderService;
		private readonly IncomingCallCallService _incomingCallService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="orderService"></param>
		/// <param name="incomingCallService"></param>
		public OrderController(
			ILogger<ApiControllerBase> logger,
			IOrderService orderService,
			IncomingCallCallService incomingCallService)
			: base(logger)
		{
			_orderService = orderService
				?? throw new ArgumentNullException(nameof(orderService));
			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
		}

		/// <summary>
		/// Создание заказа
		/// </summary>
		/// <param name="postOrderRequest"></param>
		/// <param name="unitOfWork"></param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> PostAsync(
			CreateOrderRequest postOrderRequest,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallService.GetCallByIdAsync(postOrderRequest.CallId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {postOrderRequest.CallId}", statusCode: StatusCodes.Status400BadRequest);
			}

			if(unitOfWork.GetById<DeliveryPoint>(postOrderRequest.CounterpartyId) == default)
			{
				return Problem($"Должен быть указан идентификатор контрагента, указано значение: {postOrderRequest.CounterpartyId}", statusCode: StatusCodes.Status400BadRequest);
			}

			if(unitOfWork.GetById<DeliveryPoint>(postOrderRequest.DeliveryPointId) is not DeliveryPoint deliveryPoint
				|| deliveryPoint.Counterparty.Id != postOrderRequest.CounterpartyId)
			{
				return Problem($"Должен быть указан идентификатор существующей точки доставки указанного контрагента, указано значение: {postOrderRequest.DeliveryPointId}", statusCode: StatusCodes.Status400BadRequest);
			}

			if(postOrderRequest.DeliveryDate is null
				|| postOrderRequest.DeliveryIntervalId is null
				|| postOrderRequest.SignatureType is null
				|| postOrderRequest.ContactPhone is null
				|| postOrderRequest.PaymentType is null
				|| postOrderRequest.CallBeforeArrivalMinutes is null
				|| postOrderRequest.BottlesReturn is null
				|| !postOrderRequest.SaleItems.Any())
			{
				_orderService.CreateIncompleteOrder(postOrderRequest.MapToCreateOrderRequest());

				return NoContent();
			}

			var createdOrderId = _orderService.CreateAndAcceptOrder(postOrderRequest.MapToCreateOrderRequest());
			_logger.LogInformation("Создан заказ #{OrderId}", createdOrderId);

			return NoContent();
		}

		/// <summary>
		/// Вычисление цены заказа
		/// </summary>
		/// <param name="calculatePriceRequest"></param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpPost("CalculatePrice")]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CalculatePriceResponse))]
		public async Task<IActionResult> CalculatePriceAsync(
			CalculatePriceRequest calculatePriceRequest,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallService.GetCallByIdAsync(calculatePriceRequest.CallId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {calculatePriceRequest.CallId}", statusCode: StatusCodes.Status400BadRequest);
			}

			(var orderPrice, var deliveryPrice) = _orderService.GetOrderAndDeliveryPrices(calculatePriceRequest.MapToCreateOrderRequest());

			return Ok(new CalculatePriceResponse
			{
				OrderPrice = orderPrice,
				DeliveryPrice = deliveryPrice
			});
		}
	}
}
