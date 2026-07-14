using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V6.Dto.Orders;
using CustomerOrdersApi.Library.V6.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors.Orders;
using ICourierTrackingService = CustomerOrdersApi.Library.V7.Services.ICourierTrackingService;

namespace CustomerOrdersApi.Controllers.V7
{
	/// <summary>
	/// Контроллер для работы с отслеживанием координат
	/// </summary>
	[ApiVersion("6.0")]
	[Authorize]
	public class TrackingController : VersionedController
	{
		private readonly ICourierTrackingService _courierTrackingService;

		public TrackingController(
			ILogger<TrackingController> logger,
			ICourierTrackingService courierTrackingService
			) : base(logger)
		{
			_courierTrackingService = courierTrackingService ?? throw new ArgumentNullException(nameof(courierTrackingService));
		}

		/// <summary>
		/// Получение координат курьера и точки доставки
		/// </summary>
		/// <param name="getCourierCoordinatesDto">Данные для получения координат</param>
		/// <param name="cancellationToken">Токен для отмены операции</param>
		/// <returns>Координаты курьера <see cref="CourierCoordinatesDto"/></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourierCoordinatesDto))]
		public async Task<IActionResult> GetCourierCoordinates(
			[FromBody] GetCourierCoordinatesDto getCourierCoordinatesDto,
			CancellationToken cancellationToken)
		{
			var sourceName = getCourierCoordinatesDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение координат курьера. " +
					"Клиент: {CounterpartyId} " +
					"Идентификатор клиента в ИПЗ: {ExternalCounterpartyId} " +
					"Номер заказа: {OrderId} " +
					"Номер онлайн заказа: {OnlineOrderId}",
					sourceName,
					getCourierCoordinatesDto.CounterpartyErpId,
					getCourierCoordinatesDto.ExternalCounterpartyId,
					getCourierCoordinatesDto.OrderId,
					getCourierCoordinatesDto.OnlineOrderId);

				var courierCoordinatesResult = await _courierTrackingService.GetCourierCoordinates(getCourierCoordinatesDto, cancellationToken);

				if(courierCoordinatesResult.IsFailure)
				{
					var firstError = courierCoordinatesResult.Errors.First();
					return Problem(firstError.Message, statusCode: GetStatusCode(courierCoordinatesResult));
				}

				return Ok(courierCoordinatesResult);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении координат курьера по запросу клиента {CounterpartyId} от {Source}",
					getCourierCoordinatesDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}

		private static int GetStatusCode(Result result)
		{
			if(result.IsSuccess)
			{
				return StatusCodes.Status200OK;
			}

			var firstError = result.Errors.FirstOrDefault();

			if(firstError != null
				&& (firstError.Code == OrderErrors.NotFound
					|| firstError.Code == OnlineOrderErrors.OnlineOrderNotFound
					|| firstError.Code == OnlineOrderErrors.ErpOrderForOnlineOrderNotFound))
			{
				return StatusCodes.Status404NotFound;
			}

			return StatusCodes.Status400BadRequest;
		}
	}
}
