using System;
using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class OrdersController : ControllerBase
	{
		private readonly ILogger<OrdersController> _logger;
		private readonly ICustomerOrdersService _customerOrdersService;

		public OrdersController(
			ILogger<OrdersController> logger,
			ICustomerOrdersService customerOrdersService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
		}

		[HttpPost("CreateOrder")]
		public IActionResult CreateOrder(OnlineOrderInfoDto onlineOrderInfoDto)
		{
			var sourceName = onlineOrderInfoDto.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на создание заказа для {CounterpartyId}",
					sourceName,
					onlineOrderInfoDto.CounterpartyErpId);
				
				var orderId = _customerOrdersService.CreateOrderFromOnlineOrder(onlineOrderInfoDto);

				if(orderId == default)
				{
					return BadRequest(); //отправляем код ошибки, почему не был создан заказ
				}
				
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при создании заказа для {CounterpartyId} от {Source}",
					onlineOrderInfoDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}
	}
}
