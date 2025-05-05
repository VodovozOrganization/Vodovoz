using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using WarehouseApi.Contracts.Responses.V1;
using WarehouseApi.Library.Services;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Контроллер для работы с самовывозами
	/// </summary>
	[Route("api/[controller]/[action]")]
	public class SelfDeliveryController : VersionedController
	{
		private readonly ISelfDeliveryService _selfDeliveryService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="selfDeliveryService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public SelfDeliveryController(
			ILogger<SelfDeliveryController> logger,
			ISelfDeliveryService selfDeliveryService)
			: base(logger)
		{
			_selfDeliveryService = selfDeliveryService ?? throw new ArgumentNullException(nameof(selfDeliveryService));
		}

		/// <summary>
		/// Получение информацию о заказе самовывоза по идентификатору заказа
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(typeof(GetSelfDeliveryResponse), StatusCodes.Status200OK)]
		public IActionResult ByOrderId(int orderId)
		{
			var result = _selfDeliveryService.GetSelfDeliveryOrder(orderId);

			return MapResult(result);
		}

		/// <summary>
		/// Добавление кода ЧЗ в заказ самовывоза
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="scannedCode">Сканированный код</param>
		/// <returns></returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IActionResult AddTrueMarkCode(int orderId, string scannedCode)
		{
			var result = _selfDeliveryService.AddTrueMarkCode(orderId, scannedCode);
			return MapResult(result);
		}
	}
}
