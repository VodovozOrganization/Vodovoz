using System;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Models;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class OrdersController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly IOrderModel _orderModel;

		public OrdersController(
			ILogger<CounterpartyController> logger,
			IOrderModel orderModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderModel = orderModel ?? throw new ArgumentNullException(nameof(orderModel));
		}
		
		[HttpGet("CanCounterpartyOrderPromoSetForNewClients")]
		public bool CanCounterpartyOrderPromoSetForNewClients([FromQuery] Source source, int erpCounterpartyId)
		{
			var sourceName = source.GetEnumTitle();
			var canOrder = false;
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} по проверке доступности промонаборов для новых клиентов для клиента с Id {CounterpartyId}",
					sourceName,
					erpCounterpartyId);
				canOrder = _orderModel.CanCounterpartyOrderPromoSetForNewClients(erpCounterpartyId);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при проверке доступности промонаборов для новых клиентов для клиента с Id {CounterpartyId} от {Source}",
					erpCounterpartyId,
					sourceName);
			}
			
			return canOrder;
		}
	}
}
