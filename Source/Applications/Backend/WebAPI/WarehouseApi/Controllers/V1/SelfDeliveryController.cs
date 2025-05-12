using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using WarehouseApi.Contracts.Responses.V1;
using WarehouseApi.Library.Services;
using WarehouseApi.Library.Extensions;
using WarehouseApi.Contracts.Requests.V1;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Контроллер для работы с самовывозами
	/// </summary>
	[Route("api/[controller]")]
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
			_selfDeliveryService = selfDeliveryService
				?? throw new ArgumentNullException(nameof(selfDeliveryService));
		}

		/// <summary>
		/// Получение информацию о заказе самовывоза по идентификатору документа отпуска самовывоза
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="selfDeliveryDocumentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(typeof(GetSelfDeliveryResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetAsync(
			int? orderId,
			int? selfDeliveryDocumentId)
			=> await GetDocumentByOrderIdOrSelfDeliveryDocumentId(orderId, selfDeliveryDocumentId)
				.MatchAsync<SelfDeliveryDocument, IActionResult>(
					selfDeliveryDocument =>
					{
						var nomenclatures = selfDeliveryDocument.Order.OrderItems
							.Select(x => x.Nomenclature)
							.ToArray()
							.AsEnumerable();

						return Ok(
							new GetSelfDeliveryResponse
							{
								SelfDeliveryDocumentId = selfDeliveryDocument.Id,
								Order = selfDeliveryDocument.Order.ToApiDtoV1(nomenclatures)
							});
					},
					errors => Problem(
						string.Join(", ", errors.Select(e => e.Message)),
						statusCode: StatusCodes.Status400BadRequest));

		/// <summary>
		/// Создание документа отпуска самовывоза
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPut]
		[ProducesResponseType(StatusCodes.Status201Created)]
		public async Task<IActionResult> Put(
			[FromServices] IUnitOfWork unitOfWork,
			PutSelfDeliveryRequest request)
			=> await _selfDeliveryService
				.CreateDocument(request.OrderId, request.WarehouseId)
				.BindAsync(selfDeliveryDocument =>
					_selfDeliveryService.AddCodes(selfDeliveryDocument, request.CodesToAdd))
				.BindAsync(selfDeliveryDocument => EndLoadIfNeeded(request.EndLoad, selfDeliveryDocument))
				.BindAsync(async selfDeliveryDocument =>
				{
					await unitOfWork.SaveAsync(selfDeliveryDocument);
					await unitOfWork.CommitAsync();
					return Result.Success(selfDeliveryDocument);
				})
				.MatchAsync(
					selfDeliveryDocument => Created(
						Url.Action(
							nameof(GetAsync),
							controller: nameof(SelfDeliveryController),
							new { SelfDeliveryDocumentId = selfDeliveryDocument.Id })
						, selfDeliveryDocument),
					errors => Problem(
						string.Join(", ", errors.Select(x => x.Message)),
						statusCode: StatusCodes.Status400BadRequest));


		/// <summary>
		/// Изменение документа отпуска самовывоза
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPatch]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Patch(
			[FromServices] IUnitOfWork unitOfWork,
			PatchSelfDeliveryRequest request)
			=> await _selfDeliveryService
				.GetSelfDeliveryDocumentById(request.SelfDeliveryDocumentId)
				.BindAsync(selfDeliveryDocument => _selfDeliveryService.RemoveCodes(selfDeliveryDocument, request.CodesToDelete))
				.BindAsync(selfDeliveryDocument => _selfDeliveryService.ChangeCodes(selfDeliveryDocument, request.CodesToChange))
				.BindAsync(selfDeliveryDocument => _selfDeliveryService.AddCodes(selfDeliveryDocument, request.CodesToAdd))
				.BindAsync(selfDeliveryDocument => EndLoadIfNeeded(request.EndLoad, selfDeliveryDocument))
				.BindAsync(async selfDeliveryDocument =>
				{
					await unitOfWork.SaveAsync(selfDeliveryDocument);
					await unitOfWork.CommitAsync();
					return Result.Success(selfDeliveryDocument);
				})
				.MatchAsync<SelfDeliveryDocument, IActionResult>(
					selfDeliveryDocument => NoContent(),
					errors => Problem(
						string.Join(", ", errors.Select(x => x.Message)),
						statusCode: StatusCodes.Status400BadRequest));


		private async Task<Result<SelfDeliveryDocument>> GetDocumentByOrderIdOrSelfDeliveryDocumentId(int? orderId, int? selfDeliveryDocumentId)
		{
			if(selfDeliveryDocumentId is null && orderId is null)
			{
				return Result.Failure<SelfDeliveryDocument>(new Error("Temp.Error", "Не указан идентификатор документа самовывоза или идентификатор заказа самовывоза"));
			}

			if(selfDeliveryDocumentId != null)
			{
				return await _selfDeliveryService.GetSelfDeliveryDocumentById(selfDeliveryDocumentId);
			}
			else
			{
				return await _selfDeliveryService.GetSelfDeliveryDocumentByOrderId(orderId);
			}
		}

		private Result<SelfDeliveryDocument> EndLoadIfNeeded(bool endLoadNeeded, SelfDeliveryDocument selfDeliveryDocument)
		{
			if(endLoadNeeded)
			{
				_selfDeliveryService.EndLoad(selfDeliveryDocument);
			}

			return selfDeliveryDocument;
		}
	}
}
