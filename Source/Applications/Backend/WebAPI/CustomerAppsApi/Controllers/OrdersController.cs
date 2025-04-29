using CustomerAppsApi.Library.Models;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
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
		
		[HttpGet]
		public bool CanCounterpartyOrderPromoSetForNewClients([FromBody] FreeLoaderCheckingDto freeLoaderCheckingDto)
		{
			var sourceName = freeLoaderCheckingDto.Source.GetEnumTitle();
			var canOrder = false;
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} по проверке доступности промонаборов для новых клиентов для клиента с Id {CounterpartyId}",
					sourceName,
					freeLoaderCheckingDto.ErpCounterpartyId);
				canOrder = _orderModel.CanCounterpartyOrderPromoSetForNewClients(freeLoaderCheckingDto);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при проверке доступности промонаборов для новых клиентов для клиента с Id {CounterpartyId} от {Source}",
					freeLoaderCheckingDto.ErpCounterpartyId,
					sourceName);
			}
			
			return canOrder;
		}
	}
}
