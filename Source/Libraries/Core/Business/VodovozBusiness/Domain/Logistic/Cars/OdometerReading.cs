using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "показание одометра",
		NominativePlural = "показания одометра")]
	[HistoryTrace]
	public class OdometerReading : PropertyChangedBase, IDomainObject
	{
		private Car _car;
		private DateTime _startDate;
		private DateTime? _endDate;
		private int _odometer;

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

		[Display(Name = "Одометр")]
		public virtual int Odometer
		{
			get => _odometer;
			set => SetField(ref _odometer, value);
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		public override string ToString() => $"[ТС: {Car.Id}] Показания одометра №{Id}";
	}
}
