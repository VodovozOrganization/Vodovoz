using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DriverAPI.DTOs.V4;
using GMap.NET;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
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
		private readonly ICompletedDriverWarehouseEventRepository _completedDriverWarehouseEventRepository;
		private readonly IDriverWarehouseEventQrDataHandler _driverWarehouseEventQrDataHandler;

		public DriverWarehouseEventsModel(
			ILogger<DriverWarehouseEventsModel> logger,
			IUnitOfWork unitOfWork,
			ICarRepository carRepository,
			ICompletedDriverWarehouseEventRepository completedDriverWarehouseEventRepository,
			IDriverWarehouseEventQrDataHandler driverWarehouseEventQrDataHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_completedDriverWarehouseEventRepository =
				completedDriverWarehouseEventRepository ?? throw new ArgumentNullException(nameof(completedDriverWarehouseEventRepository));
			_driverWarehouseEventQrDataHandler =
				driverWarehouseEventQrDataHandler ?? throw new ArgumentNullException(nameof(driverWarehouseEventQrDataHandler));
		}

		/// <summary>
		/// Конвертация и проверка данных из Qr кода на корректность
		/// </summary>
		/// <param name="qrData">данные Qr кода</param>
		/// <returns></returns>
		public DriverWarehouseEventQrData ConvertAndValidateQrData(string qrData)
		{
			var result = _driverWarehouseEventQrDataHandler.ConvertQrData(qrData);

			if(result.QrData is null)
			{
				if(result.ValidationResults.Any())
				{
					var sb = new StringBuilder();
					
					foreach(var validationResult in result.ValidationResults)
					{
						sb.AppendLine(validationResult.ErrorMessage);
					}
					
					_logger.LogError("Не прошли валидацию: {ValidationResult}", sb.ToString());
				}
			}

			return result.QrData;
		}
		
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <returns></returns>
		public CompletedDriverWarehouseEvent CompleteDriverWarehouseEvent(
			DriverWarehouseEventQrData qrData, DriverWarehouseEventData eventData, Employee driver)
		{
			_logger.LogInformation("Получаем событие {EventId} из QR кода от водителя {DriverName}",
				qrData.EventId,
				driver.ShortName);
			var driverWarehouseEvent = _unitOfWork.GetById<DriverWarehouseEvent>(qrData.EventId);

			var distanceMetersFromScanningLocation = 0m;
			
			_logger.LogInformation("Рассчитываем расстояние между точками для водителя {DriverName} по {EventName}",
				driver.ShortName,
				driverWarehouseEvent.EventName);
			
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
				driverWarehouseEvent.EventName);
			
			var completedEvent = new CompletedDriverWarehouseEvent
			{
				Employee = driver,
				DriverWarehouseEvent = driverWarehouseEvent,
				Car = _carRepository.GetCarByDriver(_unitOfWork, driver),
				Latitude = eventData.Latitude,
				Longitude = eventData.Longitude,
				CompletedDate = DateTime.Now,
				DocumentId = qrData.DocumentId,
				DistanceMetersFromScanningLocation = distanceMetersFromScanningLocation
			};
			
			_unitOfWork.Save(completedEvent);
			_unitOfWork.Commit();
			_logger.LogInformation("Ok");

			return completedEvent;
		}

		/// <summary>
		/// Получение списка завершенных событий за текущий день для водителя
		/// </summary>
		/// <param name="driver">водитель</param>
		/// <returns>список завершенных событий за день</returns>
		public IEnumerable<CompletedEventDto> GetTodayCompletedEventsForDriver(Employee driver)
		{
			_logger.LogInformation("Получаем завершенные события за сегодня для водителя {DriverName}", driver.ShortName);
			return _completedDriverWarehouseEventRepository.GetTodayCompletedEventsForEmployee(_unitOfWork, driver.Id);
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
