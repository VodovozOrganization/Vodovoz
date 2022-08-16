using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "история цены топлива",
		Nominative = "история цены топлива")]
	[HistoryTrace]
	public class FuelPriceVersion : PropertyChangedBase, IDomainObject
	{

		private DateTime _startDate;
		private DateTime? _endDate;
		private decimal _fuelPrice;
		private FuelType _fuelType;

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

		[Display(Name = "Цена топлива")]
		public virtual decimal FuelPrice
		{
			get => _fuelPrice;
			set => SetField(ref _fuelPrice, value);
		}
		public virtual FuelType FuelType
		{
			get => _fuelType;
			set => SetField(ref _fuelType, value);
		}
	}
}
