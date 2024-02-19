using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Logistic.Drivers
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "завершенные события нахождения водителя на складе",
		Nominative = "завершенное событие нахождения водителя на складе")]
	[HistoryTrace]
	public class CompletedDriverWarehouseEvent : PropertyChangedBase, IDomainObject
	{
		private decimal? _latitude;
		private decimal? _longitude;
		private DateTime _completedDate;
		private decimal? _distanceMetersFromScanningLocation;
		private DriverWarehouseEvent _driverWarehouseEvent;
		private Employee _employee;
		private Car _car;
		private int? _documentId;

		public virtual int Id { get; set; }

		[Display(Name = "Время события")]
		public virtual DateTime CompletedDate
		{
			get => _completedDate;
			set => SetField(ref _completedDate, value);
		}
		
		[Display(Name = "Широта")]
		public virtual decimal? Latitude
		{
			get => _latitude;
			set => SetField(ref _latitude, value);
		}
		
		[Display(Name = "Долгота")]
		public virtual decimal? Longitude
		{
			get => _longitude;
			set => SetField(ref _longitude, value);
		}
		
		[Display(Name = "Расстояние от места сканирования (м)")]
		public virtual decimal? DistanceMetersFromScanningLocation
		{
			get => _distanceMetersFromScanningLocation;
			set => SetField(ref _distanceMetersFromScanningLocation, value);
		}

		[Display(Name = "Событие")]
		public virtual DriverWarehouseEvent DriverWarehouseEvent
		{
			get => _driverWarehouseEvent;
			set => SetField(ref _driverWarehouseEvent, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}
		
		[Display(Name = "Номер документа")]
		public virtual int? DocumentId
		{
			get => _documentId;
			set => SetField(ref _documentId, value);
		}
	}
}
