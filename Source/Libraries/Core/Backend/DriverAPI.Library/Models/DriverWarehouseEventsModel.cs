using System;
using DriverAPI.DTOs.V4;
using GMap.NET;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Tools.Logistic;

namespace DriverAPI.Library.Models
{
	public class DriverWarehouseEventsModel : IDriverWarehouseEventsModel
	{
		private readonly ILogger<DriverWarehouseEventsModel> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICarRepository _carRepository;

		public DriverWarehouseEventsModel(
			ILogger<DriverWarehouseEventsModel> logger,
			IUnitOfWork unitOfWork,
			ICarRepository carRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
		}
		
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <returns></returns>
		public void CompleteDriverWarehouseEvent(DriverWarehouseEventData eventData, Employee driver)
		{
			_logger.LogInformation("Получаем событие {EventId} из QR кода от водителя {DriverName}",
				eventData.DriverWarehouseEventId,
				driver.ShortName);
			var driverWarehouseEvent = _unitOfWork.GetById<DriverWarehouseEvent>(eventData.DriverWarehouseEventId);

			var distanceMetersFromScanningLocation = 0m;
			
			_logger.LogInformation("Рассчитываем расстояние между точками для водителя {DriverName} по {EventName}",
				driver.ShortName,
				driverWarehouseEvent.EventName.Name);

			if(!eventData.Latitude.HasValue || !eventData.Longitude.HasValue)
			{
				distanceMetersFromScanningLocation = 0m;
			}
			else if(driverWarehouseEvent.Type == DriverWarehouseEventType.OnLocation
				&& driverWarehouseEvent.Latitude.HasValue
				&& driverWarehouseEvent.Longitude.HasValue)
			{
				var driverPoint = GetPointLatLng(eventData.Latitude, eventData.Longitude);
				var qrPoint = GetPointLatLng(driverWarehouseEvent.Latitude, driverWarehouseEvent.Longitude);
			
				distanceMetersFromScanningLocation = (decimal)DistanceCalculator.GetDistance(driverPoint, qrPoint);
			}

			_logger.LogInformation("Создаем завершенное событие для водителя {DriverName} по {EventName}",
				driver.ShortName,
				driverWarehouseEvent.EventName.Name);
			
			var completedEvent = new CompletedDriverWarehouseEvent
			{
				Employee = driver,
				DriverWarehouseEvent = driverWarehouseEvent,
				Car = _carRepository.GetCarByDriver(_unitOfWork, driver),
				Latitude = eventData.Latitude,
				Longitude = eventData.Longitude,
				CompletedDate = DateTime.Now,
				DocumentType = eventData.DocumentType,
				DocumentId = eventData.DocumentId,
				DistanceMetersFromScanningLocation = distanceMetersFromScanningLocation
			};
			
			_unitOfWork.Save(completedEvent);
			_unitOfWork.Commit();
			_logger.LogInformation("Ok");
		}

		private PointLatLng GetPointLatLng(decimal? latitude, decimal? longitude)
		{
			var pointLatitude = latitude.HasValue
				? Convert.ToDouble(latitude)
				: 0d;

			var pointLongitude = longitude.HasValue
				? Convert.ToDouble(longitude)
				: 0d;
			
			return new PointLatLng(pointLatitude, pointLongitude);
		}
	}
}
