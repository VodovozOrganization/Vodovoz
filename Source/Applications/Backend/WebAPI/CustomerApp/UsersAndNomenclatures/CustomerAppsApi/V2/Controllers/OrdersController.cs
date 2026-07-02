using System;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using CustomerAppsApi.Library.V2.Models;
using Gamma.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.V2.Controllers
{
	[Authorize]
	[ApiVersion("2.0")]
	public class OrdersController : VersionedController
	{
		private readonly IOrderModel _orderModel;

		public OrdersController(
			ILogger<OrdersController> logger,
			IOrderModel orderModel) : base(logger)
		{
			_orderModel = orderModel ?? throw new ArgumentNullException(nameof(orderModel));
		}
		
		/// <summary>
		/// Может ли клиент заказывать промонаборы для новых клиентов
		/// </summary>
		/// <param name="freeLoaderCheckingDto">Входящие данные для проверки <see cref="FreeLoaderCheckingDto"/></param>
		/// <returns><c>true</c> - да, <c>false</c> - нет</returns>
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
