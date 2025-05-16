using System;
using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers
{
	/// <summary>
	/// Контроллер для работы с отображением курьера на карте
	/// </summary>
	public class TrackingCourierController : SignatureControllerBase
	{
		private readonly ICustomerOrdersTrackingCourierService _trackingCourierService;

		public TrackingCourierController(
			ILogger<SignatureControllerBase> logger,
			ICustomerOrdersTrackingCourierService trackingCourierService) : base(logger)
		{
			_trackingCourierService = trackingCourierService ?? throw new ArgumentNullException(nameof(trackingCourierService));
		}

		/// <summary>
		/// Получение текущих координат курьера(водителя)
		/// </summary>
		/// <param name="coordinatesRequest">Данные запроса <see cref="CourierCoordinatesRequest"/></param>
		/// <returns>Данные с координатами курьера и адреса по заказу</returns>
		[HttpGet]
		public IActionResult GetCurrentCourierCoordinates([FromBody] CourierCoordinatesRequest coordinatesRequest)
		{
			var sourceName = coordinatesRequest.Source.GetEnumTitle();
			
			try
			{
				Logger.LogInformation(
					"Поступил запрос от {Source} на получение текущих координат курьера(водителя)" +
					" по заказу {ExternalOrderId} c подписью {Signature}, проверяем...",
					sourceName,
					coordinatesRequest.ExternalOrderId,
					coordinatesRequest.Signature);
				
				if(!_trackingCourierService.ValidateCourierCoordinatesSignature(coordinatesRequest, out var generatedSignature))
				{
					return InvalidSignature(coordinatesRequest.Signature, generatedSignature);
				}

				Logger.LogInformation(
					"Подпись валидна, получаем координаты по заказу {ExternalOrderId}",
					coordinatesRequest.ExternalOrderId);
				
				return Ok(_trackingCourierService.GetCurrentCourierCoordinates(coordinatesRequest));
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при получении координат курьера по заказу {ExternalOrderId} от {Source}",
					coordinatesRequest.ExternalOrderId,
					sourceName);

				return Problem();
			}
		}
	}
}
