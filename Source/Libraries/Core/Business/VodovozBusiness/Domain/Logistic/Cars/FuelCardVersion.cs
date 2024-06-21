using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "версия топливной карты",
		NominativePlural = "версии топливных карт",
		Genitive = "версии топливной карты",
		GenitivePlural = "версий топливных карт")]
	[HistoryTrace]
	public class FuelCardVersion : PropertyChangedBase, IDomainObject
	{
		private DateTime _startDate;
		private DateTime? _endDate;
		private Car _car;
		private FuelCard _fuelCard;

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

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		[Display(Name = "Топливная карта")]
		public virtual FuelCard FuelCard
		{
			get => _fuelCard;
			set => SetField(ref _fuelCard, value);
		}
	}
}
