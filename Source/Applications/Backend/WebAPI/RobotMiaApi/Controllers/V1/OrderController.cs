using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RobotMiaApi.Contracts.Requests.V1;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Extensions.Mapping;
using System;
using System.Net.Mime;
using Vodovoz.Presentation.WebApi.Common;
using VodovozBusiness.Services.Orders;

namespace RobotMiaApi.Controllers.V1
{
	/// <summary>
	/// Контроллер заказов
	/// </summary>
	public class OrderController : VersionedController
	{
		private readonly IOrderService _orderService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="orderService"></param>
		public OrderController(
			ILogger<ApiControllerBase> logger,
			IOrderService orderService)
			: base(logger)
		{
			_orderService = orderService
				?? throw new ArgumentNullException(nameof(orderService));
		}

		/// <summary>
		/// Создание заказа
		/// </summary>
		/// <param name="postOrderRequest"></param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IActionResult Post(Contracts.Requests.V1.CreateOrderRequest postOrderRequest)
		{
			var createdOrderId = _orderService.CreateAndAcceptOrder(postOrderRequest.MapToCreateOrderRequest());

			_logger.LogInformation("Создан заказ #{OrderId}", createdOrderId);

			return NoContent();
		}

		/// <summary>
		/// Вычисление цены заказа
		/// </summary>
		/// <param name="calculatePriceRequest"></param>
		/// <returns></returns>
		[HttpPost("CalculatePrice")]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public CalculatePriceResponse CalculatePrice(CalculatePriceRequest calculatePriceRequest)
		{
			(var orderPrice, var deliveryPrice) = _orderService.GetOrderAndDeliveryPrices(calculatePriceRequest.MapToCreateOrderRequest());

			return new CalculatePriceResponse
			{
				OrderPrice = orderPrice,
				DeliveryPrice = deliveryPrice
			};
		}
	}
}
