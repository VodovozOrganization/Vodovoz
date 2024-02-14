﻿using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "операторы",
		Nominative = "оператор")]
	[EntityPermission]
	[HistoryTrace]
	public class Operator : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private WorkShift _workShift;
		private bool _pacsEnabled;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Рабочая смена")]
		public virtual WorkShift WorkShift
		{
			get => _workShift;
			set => SetField(ref _workShift, value);
		}

		[Display(Name = "Включен в СКУД")]
		public virtual bool PacsEnabled
		{
			get => _pacsEnabled;
			set => SetField(ref _pacsEnabled, value);
		}


		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Id == 0)
			{
				yield return new ValidationResult("Необходимо выбрать оператора", new[] { nameof(Operator) });
			}

			if(WorkShift == null)
			{
				yield return new ValidationResult("Необходимо выбрать смену", new[] { nameof(WorkShift) });
			}
		}
	}

	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "смены",
		Nominative = "смена")]
	[EntityPermission]
	[HistoryTrace]
	public class WorkShift : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private string _name;
		private TimeSpan _duration;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}


		[Display(Name = "Длительность")]
		public virtual TimeSpan Duration
		{
			get => _duration;
			set => SetField(ref _duration, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Duration <= TimeSpan.Zero)
			{
				yield return new ValidationResult("Длительность должна быть больше нуля", new[] { nameof(Duration) });
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено", new[] { nameof(Name) });
			}
		}
	}
}
