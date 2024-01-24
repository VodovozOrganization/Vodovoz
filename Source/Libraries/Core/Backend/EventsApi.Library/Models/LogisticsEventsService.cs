using System;
using System.Collections.Generic;
using EventsApi.Library.Dtos;
using EventsApi.Library.Services;
using GMap.NET;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Data.Interfaces.Logistics.Cars;
using Vodovoz.Core.Data.Logistics;
using Vodovoz.Core.Domain;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace EventsApi.Library.Models
{
	public abstract class LogisticsEventsService : ILogisticsEventsService
	{
		private readonly ILogger _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDriverWarehouseEventQrDataHandler _driverWarehouseEventQrDataHandler;
		private readonly ICarIdRepository _carIdRepository;
		private readonly ICompletedDriverWarehouseEventProxyRepository _completedDriverWarehouseEventProxyRepository;
		private readonly IEmployeeWithLoginRepository _employeeWithLoginRepository;
		private readonly EmployeeType _employeeType;
		
		protected LogisticsEventsService(
			ILogger logger,
			IUnitOfWork unitOfWork,
			ICompletedDriverWarehouseEventProxyRepository completedDriverWarehouseEventProxyRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			IDriverWarehouseEventQrDataHandler driverWarehouseEventQrDataHandler,
			EmployeeType employeeType,
			ICarIdRepository carIdRepository = null)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_driverWarehouseEventQrDataHandler =
				driverWarehouseEventQrDataHandler ?? throw new ArgumentNullException(nameof(driverWarehouseEventQrDataHandler));
			_completedDriverWarehouseEventProxyRepository =
				completedDriverWarehouseEventProxyRepository ?? throw new ArgumentNullException(nameof(completedDriverWarehouseEventProxyRepository));
			_employeeWithLoginRepository =
				employeeWithLoginRepository ?? throw new ArgumentNullException(nameof(employeeWithLoginRepository));
			_employeeType = employeeType;
			_carIdRepository = carIdRepository;
		}
		
		/// <summary>
		/// Конвертация и проверка данных из Qr кода на корректность
		/// </summary>
		/// <param name="qrData">данные Qr кода</param>
		/// <returns></returns>
		public DriverWarehouseEventQrData ConvertAndValidateQrData(string qrData)
		{
			_logger.LogInformation("Проверяем данные Qr кода на валидность");
			var result = _driverWarehouseEventQrDataHandler.ConvertQrData(qrData);

			return result;
		}
		
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <returns></returns>
		public CompletedDriverWarehouseEventProxy CompleteDriverWarehouseEvent(
			DriverWarehouseEventQrData qrData,
			DriverWarehouseEventData eventData,
			EmployeeWithLogin employee)
		{
			_logger.LogInformation("Получаем событие {EventId} из QR кода от {EmployeeType} {EmployeeName}",
				qrData.EventId,
				_employeeType.ToString(),
				employee.ShortName);
			var driverWarehouseEvent = _unitOfWork.GetById<DriverWarehouseEvent>(qrData.EventId);

			var distanceMetersFromScanningLocation = 0m;
			
			_logger.LogInformation("Рассчитываем расстояние между точками для {EmployeeName} по событию {EventName}",
				employee.ShortName,
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
			
				distanceMetersFromScanningLocation = (decimal)DistanceCalculator.GetDistanceMeters(driverPoint, qrPoint);
			}

			_logger.LogInformation("Создаем завершенное событие {EventName} для {EmployeeName}",
				driverWarehouseEvent.EventName,
				employee.ShortName);

			var completedEvent = CreateCompletedEvent(qrData, eventData, employee, driverWarehouseEvent, distanceMetersFromScanningLocation);
			
			_unitOfWork.Save(completedEvent);
			_unitOfWork.Commit();
			_logger.LogInformation("Ok");

			return completedEvent;
		}

		/// <summary>
		/// Получение списка завершенных событий за текущий день для сотрудника
		/// </summary>
		/// <param name="employee">сотрудник</param>
		/// <returns>список завершенных событий за день</returns>
		public IEnumerable<CompletedEventDto> GetTodayCompletedEvents(EmployeeWithLogin employee)
		{
			_logger.LogInformation(
				"Получаем завершенные события за сегодня для пользователя {EmployeeType} {EmployeeName}",
				_employeeType.ToString(),
				employee.ShortName);
			
			return _completedDriverWarehouseEventProxyRepository.GetTodayCompletedEventsForEmployee(_unitOfWork, employee.Id);
		}

		public EmployeeWithLogin GetEmployeeProxyByApiLogin(
			string userLogin,
			ExternalApplicationType applicationType = ExternalApplicationType.WarehouseApp)
			=> _employeeWithLoginRepository.GetEmployeeWithLogin(_unitOfWork, userLogin, applicationType);

		protected PointLatLng GetPointLatLng(decimal? latitude, decimal? longitude)
		{
			var pointLatitude = latitude.HasValue
				? Convert.ToDouble(latitude)
				: 0d;

			var pointLongitude = longitude.HasValue
				? Convert.ToDouble(longitude)
				: 0d;
			
			return new PointLatLng(pointLatitude, pointLongitude);
		}
		
		private CompletedDriverWarehouseEventProxy CreateCompletedEvent(
			DriverWarehouseEventQrData qrData,
			DriverWarehouseEventData eventData,
			EmployeeWithLogin employee,
			DriverWarehouseEvent driverWarehouseEvent,
			decimal distanceMetersFromScanningLocation)
		{
			int? carId = null;
			
			if(_carIdRepository != null)
			{
				carId = _carIdRepository.GetCarIdByEmployeeId(_unitOfWork, employee.Id);
			}
			
			var completedEvent = CompletedDriverWarehouseEventProxy.Create(
				eventData.Latitude,
				eventData.Longitude,
				distanceMetersFromScanningLocation,
				driverWarehouseEvent,
				employee,
				qrData.DocumentId,
				carId
				);

			return completedEvent;
		}
	}
}
