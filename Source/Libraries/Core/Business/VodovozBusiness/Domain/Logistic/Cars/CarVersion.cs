using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "версия автомобиля",
		NominativePlural = "версии автомобиля")]
	[HistoryTrace]
	public class CarVersion : PropertyChangedBase, IDomainObject
	{
		private Car _car;
		private DateTime _startDate;
		private DateTime? _endDate;
		private CarOwnType _carOwnType;
		private Organization _carOwnerOrganization;

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

		[Display(Name = "Принадлежность")]
		public virtual CarOwnType CarOwnType
		{
			get => _carOwnType;
			set => SetField(ref _carOwnType, value);
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		[Display(Name = "Собственник авто")]
		public virtual Organization CarOwnerOrganization
		{
			get => _carOwnerOrganization;
			set => SetField(ref _carOwnerOrganization, value);
		}

		public virtual bool IsCompanyCar => CarOwnType == CarOwnType.Company;
		public virtual bool IsRaskat => CarOwnType == CarOwnType.Raskat;

		public override string ToString() => $"[ТС: {Car.Id}] Версия авто №{Id}";
	}
}
