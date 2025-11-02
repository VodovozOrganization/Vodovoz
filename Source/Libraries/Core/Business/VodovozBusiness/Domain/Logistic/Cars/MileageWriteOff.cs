using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "списания километража",
		Nominative = "списание километража",
		Genitive = "списания километража")]
	[EntityPermission]
	[HistoryTrace]
	public class MileageWriteOff : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _commentMaxLength = 500;

		private DateTime _creationDate;
		private DateTime? _writeOffDate;
		private decimal _distanceKm;
		private decimal _litersOutlayed;
		private MileageWriteOffReason _reason;
		private Car _car;
		private Employee _driver;
		private Employee _author;
		private string _comment;

		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Дата списания километража")]
		public virtual DateTime? WriteOffDate
		{
			get => _writeOffDate;
			set => SetField(ref _writeOffDate, value);
		}

		[Display(Name = "Списываемое расстояние")]
		public virtual decimal DistanceKm
		{
			get => _distanceKm;
			set => SetField(ref _distanceKm, value);
		}

		[Display(Name = "Потрачено топлива, литров")]
		public virtual decimal LitersOutlayed
		{
			get => _litersOutlayed;
			set => SetField(ref _litersOutlayed, value);
		}

		[Display(Name = "Причина списания")]
		public virtual MileageWriteOffReason Reason
		{
			get => _reason;
			set => SetField(ref _reason, value);
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		public virtual string Title => $"Списание километража №{Id}";

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(DistanceKm == 0)
			{
				yield return new ValidationResult(
					"Списываемое расстояние должно быть больше 0",
					new[] { nameof(DistanceKm) });
			}

			if(LitersOutlayed == 0)
			{
				yield return new ValidationResult(
					"Значение количества потраченного топлива должно быть больше 0",
					new[] { nameof(LitersOutlayed) });
			}

			if(Reason is null)
			{
				yield return new ValidationResult(
					"Необходимо указать причину списания",
					new[] { nameof(Reason) });
			}

			if(Car is null)
			{
				yield return new ValidationResult(
					"Необходимо указать автомобиль",
					new[] { nameof(Car) });
			}

			if(Driver is null)
			{
				yield return new ValidationResult(
					"Необходимо указать водителя",
					new[] { nameof(Driver) });
			}

			if(Author is null)
			{
				yield return new ValidationResult(
					"Необходимо указать автора",
					new[] { nameof(Author) });
			}

			if(Comment?.Length > _commentMaxLength)
			{
				yield return new ValidationResult(
					$"Комментарий не должно быть длиннее {_commentMaxLength} символов",
					new[] { nameof(Comment) });
			}

			if(!WriteOffDate.HasValue)
			{
				yield return new ValidationResult(
					"Необходимо указать дату списания",
					new[] { nameof(WriteOffDate) });
			}

			if(CreationDate == default)
			{
				yield return new ValidationResult(
					"Необходимо указать дату создания",
					new[] { nameof(CreationDate) });
			}
		}
	}
}
