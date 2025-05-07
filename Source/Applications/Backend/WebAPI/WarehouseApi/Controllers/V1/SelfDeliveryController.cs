using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Orders;
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
	[Route("api/[controller]")]
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
		/// Получение информацию о заказе самовывоза по идентификатору документа отпуска самовывоза
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="orderId"></param>
		/// <param name="selfDeliveryDocumentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(typeof(GetSelfDeliveryResponse), StatusCodes.Status200OK)]
		public IActionResult Get(
			[FromServices] IUnitOfWork unitOfWork,
			int? orderId,
			int? selfDeliveryDocumentId)
		{
			var selfDeliveryDocument = _selfDeliveryDocumentRepository
				.Get(
					unitOfWork,
					x => x.Id == selfDeliveryDocumentId,
					1)
				.FirstOrDefault();

			throw new NotImplementedException("Метод не реализован");

			//return (selfDeliveryDocument.ToApiDtoV1());
		}

		///// <summary>
		///// Получение информацию о заказе самовывоза по идентификатору заказа
		///// </summary>
		///// <param name="unitOfWork"></param>
		///// <param name="orderId">Идентификатор заказа</param>
		///// <param name="cancellationToken"></param>
		///// <returns></returns>
		//[HttpGet()]
		//[Produces(MediaTypeNames.Application.Json)]
		//[ProducesResponseType(typeof(GetSelfDeliveryResponse), StatusCodes.Status200OK)]
		//public IActionResult ByOrderId(
		//	[FromServices] IUnitOfWork unitOfWork,
		//	int orderId,
		//	CancellationToken cancellationToken)
		//{
		//	var getOrderResult = _selfDeliveryService.GetSelfDeliveryOrder(orderId);

		//	if(getOrderResult.IsFailure)
		//	{
		//		return MapResult(getOrderResult);
		//	}

		//	var result = new GetSelfDeliveryResponse
		//	{
		//		Order = getOrderResult.Value
		//	};

		//	if(cancellationToken.IsCancellationRequested)
		//	{
		//		_logger.LogWarning(
		//			"Cancellation requested for orderId {OrderId}",
		//			orderId);

		//		return NoContent();
		//	}

		//	var selfDeliveryDocument = _selfDeliveryDocumentRepository
		//		.Get(
		//			unitOfWork,
		//			x => x.Order.Id == orderId,
		//			1)
		//		.FirstOrDefault();

		//	result.SelfDeliveryDocumentId = selfDeliveryDocument?.Id;

		//	return MapResult(Result.Success(result));
		//}

		///// <summary>
		///// Добавление кода ЧЗ в заказ самовывоза
		///// </summary>
		///// <param name="orderId">Идентификатор заказа</param>
		///// <param name="scannedCode">Сканированный код</param>
		///// <param name="cancellationToken"></param>
		///// <returns></returns>
		//[HttpPost]
		//[ProducesResponseType(StatusCodes.Status204NoContent)]
		//public async Task<IActionResult> AddTrueMarkCodeAsync(
		//	int orderId,
		//	string scannedCode,
		//	CancellationToken cancellationToken)
		//{
		//	var result = await _selfDeliveryService.AddTrueMarkCode(orderId, scannedCode, cancellationToken);
		//	return MapResult(result);
		//}


		[HttpPut]
		[ProducesResponseType(StatusCodes.Status201Created)]
		public async Task<IActionResult> Put(
			[FromServices] IUnitOfWork unitOfWork,
			PutSelfDeliveryRequest request)
		{
			_selfDeliveryService.CreateDocument();

			if(request.CodesToAdd.Any())
			{
				_selfDeliveryService.AddCodes(request.CodesToAdd);
			}

			if(request.EndLoad)
			{
				_selfDeliveryService.EndLoad();
			}
		}


		[HttpPatch]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Patch(
			[FromServices] IUnitOfWork unitOfWork,
			PatchSelfDeliveryRequest request)
		{
			_selfDeliveryService
				.RemoveCodes(request.CodesToDelete)
				.ChangeCodes(request.CodesToChange)
				.AddCodes(request.CodesToAdd);

			//if(request.CodesToDelete.Any())
			//{
			//	_selfDeliveryService.RemoveCodes(request.CodesToDelete);
			//}

			//if(request.CodesToChange.Any())
			//{
			//	_selfDeliveryService.ChangeCodes(request.CodesToChange);
			//}

			//if(request.CodesToAdd.Any())
			//{
			//	_selfDeliveryService.AddCodes(request.CodesToAdd);
			//}

			if(request.EndLoad)
			{
				_selfDeliveryService.EndLoad();
			}

			unitOfWork.Commit();

			throw new NotImplementedException("Метод не реализован");
		}

		//[HttpDelete]
		//[ProducesResponseType(StatusCodes.Status204NoContent)]
		//public async Task<IActionResult> DeleteTrueMarkCode()
		//{
		//	throw new NotImplementedException("Метод не реализован");
		//}

		///// <summary>
		///// Завершение отгрузки самовывоза
		///// </summary>
		///// <returns></returns>
		///// <exception cref="NotImplementedException"></exception>
		//[HttpPost]
		//[ProducesResponseType(StatusCodes.Status204NoContent)]
		//public IActionResult EndLoad(int selfDeliveryDocumentId)
		//{
		//	throw new NotImplementedException("Метод не реализован");
		//}
	}

	public class PutSelfDeliveryRequest
	{
	}

	public class PatchSelfDeliveryRequest
	{
		public OrderDto Order { get; set; }
		public bool EndLoad { get; set; }
	}
}
