﻿using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

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

		public virtual bool IsCompanyCar => CarOwnType == CarOwnType.Company;
		public virtual bool IsRaskat => CarOwnType == CarOwnType.Raskat;

		public override string ToString() => $"[ТС: {Car.Id}] Версия авто №{Id}";
	}

	public enum CarOwnType
	{
		[Display(Name = "ТС компании")]
		Company,
		[Display(Name = "ТС в раскате")]
		Raskat,
		[Display(Name = "ТС водителя")]
		Driver
	}

	public class CarOwnTypeStringType : NHibernate.Type.EnumStringType
	{
		public CarOwnTypeStringType() : base(typeof(CarOwnType))
		{ }
	}
}
