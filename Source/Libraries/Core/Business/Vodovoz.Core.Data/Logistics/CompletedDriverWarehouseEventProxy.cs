using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.Logistics
{
	public class CompletedDriverWarehouseEventProxy : IDomainObject
	{
		//Нужен для Nhibernate
		protected CompletedDriverWarehouseEventProxy() { }
		
		private CompletedDriverWarehouseEventProxy(
			decimal? latitude,
			decimal? longitude,
			decimal? distanceMetersFromScanningLocation,
			DriverWarehouseEvent @event,
			EmployeeWithLogin employee,
			int? documentId,
			int? carId = null)
		{
			Latitude = latitude;
			Longitude = longitude;
			DistanceMetersFromScanningLocation = distanceMetersFromScanningLocation;
			DriverWarehouseEvent = @event;
			Employee = employee;
			CarId = carId;
			DocumentId = documentId;
			CompletedDate = DateTime.Now;
		}
		
		public virtual int Id { get; }

		[Display(Name = "Время события")]
		public virtual DateTime CompletedDate { get; }
		
		[Display(Name = "Широта")]
		public virtual decimal? Latitude { get; }
		
		[Display(Name = "Долгота")]
		public virtual decimal? Longitude { get; }
		
		[Display(Name = "Расстояние от места сканирования (м)")]
		public virtual decimal? DistanceMetersFromScanningLocation { get; }

		[Display(Name = "Событие")]
		public virtual DriverWarehouseEvent DriverWarehouseEvent { get; }

		[Display(Name = "Автор")]
		public virtual EmployeeWithLogin Employee { get; }

		[Display(Name = "Автомобиль")]
		public virtual int? CarId { get; }
		
		[Display(Name = "Номер документа")]
		public virtual int? DocumentId { get; }

		public static CompletedDriverWarehouseEventProxy Create(
			decimal? latitude,
			decimal? longitude,
			decimal? distanceMetersFromScanningLocation,
			DriverWarehouseEvent @event,
			EmployeeWithLogin employee,
			int? documentId,
			int? carId = null)
		{
			return new CompletedDriverWarehouseEventProxy(latitude, longitude, distanceMetersFromScanningLocation, @event, employee,
				documentId, carId);
		}
	}
}
