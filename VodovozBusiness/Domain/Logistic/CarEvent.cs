using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "события ТС",
		Nominative = "событие ТС")]
	[EntityPermission]
	[HistoryTrace]

	public class CarEvent : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _createDate;
		private Employee _author;
		private CarEventType _carEventType;
		private Car _car;
		private Employee _driver;
		private DateTime _startDate;
		private DateTime _endDate;
		private string _comment;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Required(ErrorMessage = "Вид события ТС должен быть указан.")]
		[Display(Name = "Вид события ТС")]
		public virtual CarEventType CarEventType
		{
			get => _carEventType;
			set => SetField(ref _carEventType, value);
		}

		[Required(ErrorMessage = "Автомобиль должен быть указан.")]
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

		[Required(ErrorMessage = "Дата начала события ТС должна быть указана.")]
		[Display(Name = "Дата начала события ТС")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Required(ErrorMessage = "Дата окончания события ТС должна быть указана.")]
		[Display(Name = "Дата окончания события ТС")]
		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		#endregion

		public override string ToString()
		{
			return $"Событие ТС №{Id} {CarEventType.ShortName}";
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == default(DateTime))
			{
				yield return new ValidationResult(String.Format("Дата начала события должна быть указана."),
					new[] { nameof(StartDate) });
			}

			if(EndDate == default(DateTime))
			{
				yield return new ValidationResult("Дата окончания события должна быть указана.",
					new[] { nameof(EndDate) });
			}

			if(StartDate > EndDate)
			{
				yield return new ValidationResult("Дата окончания должна быть больше даты начала.",
					new[] { nameof(StartDate), nameof(EndDate) });
			}

			if(CarEventType.NeedComment && string.IsNullOrEmpty(Comment))
			{
				yield return new ValidationResult("Комментарий должен быть заполнен.",
						new[] { nameof(Comment) });
			}
		}

		#endregion
	}
}
