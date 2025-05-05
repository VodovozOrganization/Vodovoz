using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Documents;
using Vodovoz.Errors;
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
		private readonly IGenericRepository<SelfDeliveryDocument> _selfDeliveryDocumentRepository;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="selfDeliveryService"></param>
		/// <param name="selfDeliveryDocumentRepository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public SelfDeliveryController(
			ILogger<SelfDeliveryController> logger,
			ISelfDeliveryService selfDeliveryService,
			IGenericRepository<SelfDeliveryDocument> selfDeliveryDocumentRepository)
			: base(logger)
		{
			_selfDeliveryService = selfDeliveryService
				?? throw new ArgumentNullException(nameof(selfDeliveryService));
			_selfDeliveryDocumentRepository = selfDeliveryDocumentRepository
				?? throw new ArgumentNullException(nameof(selfDeliveryDocumentRepository));
		}

		/// <summary>
		/// Получение информацию о заказе самовывоза по идентификатору заказа
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(typeof(GetSelfDeliveryResponse), StatusCodes.Status200OK)]
		public IActionResult ByOrderId([FromServices] IUnitOfWork unitOfWork, int orderId, CancellationToken cancellationToken)
		{
			var getOrderResult = _selfDeliveryService.GetSelfDeliveryOrder(orderId);

			if(getOrderResult.IsFailure)
			{
				return MapResult(getOrderResult);
			}

			var result = new GetSelfDeliveryResponse
			{
				Order = getOrderResult.Value
			};

			if(cancellationToken.IsCancellationRequested)
			{
				_logger.LogWarning(
					"Cancellation requested for orderId {OrderId}",
					orderId);

				return NoContent();
			}

			if(_selfDeliveryDocumentRepository
				.Get(
					unitOfWork,
					x => x.Order.Id == orderId,
					1)
				.FirstOrDefault()
				!= null)
			{
				result.SelfDeliveryDocumentExists = true;
			}
			
			return MapResult(Result.Success(result));
		}

		/// <summary>
		/// Добавление кода ЧЗ в заказ самовывоза
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="scannedCode">Сканированный код</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> AddTrueMarkCodeAsync(int orderId, string scannedCode, CancellationToken cancellationToken)
		{
			var result = await _selfDeliveryService.AddTrueMarkCode(orderId, scannedCode, cancellationToken);
			return MapResult(result);
		}
	}
}
