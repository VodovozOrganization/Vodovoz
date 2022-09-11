using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "версия расхода топлива",
		NominativePlural = "версии расхода топлива")]
	[HistoryTrace]
	public class CarFuelVersion : PropertyChangedBase, IDomainObject
	{
		private DateTime _startDate;
		private DateTime? _endDate;
		private double _fuelConsumption;
		private CarModel _carModel;

		public virtual int Id { get; set; }

		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Расход топлива")]
		public virtual double FuelConsumption
		{
			get => _fuelConsumption;
			set => SetField(ref _fuelConsumption, value);
		}
		public virtual CarModel CarModel
		{
			get => _carModel;
			set => SetField(ref _carModel, value);
		}
	}
}
