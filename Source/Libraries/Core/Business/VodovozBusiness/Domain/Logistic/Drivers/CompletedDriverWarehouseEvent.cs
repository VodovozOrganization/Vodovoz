using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Logistic.Drivers
{
	[HistoryTrace]
	public class CompletedDriverWarehouseEvent : PropertyChangedBase, IDomainObject
	{
		private decimal? _latitude;
		private decimal? _longitude;
		private DateTime _completedDate;
		private decimal? _distanceMetersBetweenPoints;
		private DriverWarehouseEvent _driverWarehouseEvent;
		private Employee _driver;
		private Car _car;

		public virtual int Id { get; set; }

		public virtual DateTime CompletedDate
		{
			get => _completedDate;
			set => SetField(ref _completedDate, value);
		}
		
		public virtual decimal? Latitude
		{
			get => _latitude;
			set => SetField(ref _latitude, value);
		}
		
		public virtual decimal? Longitude
		{
			get => _longitude;
			set => SetField(ref _longitude, value);
		}
		
		[Display(Name = "Расстояние между точками(сканирования и QR кода)")]
		public virtual decimal? DistanceMetersBetweenPoints
		{
			get => _distanceMetersBetweenPoints;
			set => SetField(ref _distanceMetersBetweenPoints, value);
		}

		public virtual DriverWarehouseEvent DriverWarehouseEvent
		{
			get => _driverWarehouseEvent;
			set => SetField(ref _driverWarehouseEvent, value);
		}

		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}
	}
}
