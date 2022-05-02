using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using NHibernate.Type;

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
		private bool _doNotShowInOperation;
		private Insurance _insurance;
		private decimal _price;

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

		[Display(Name = "Вид события ТС")]
		public virtual CarEventType CarEventType
		{
			get => _carEventType;
			set => SetField(ref _carEventType, value);
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

		[Display(Name = "Дата начала события ТС")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

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

		[Display( Name = "Не отражать в эксплуатации ТС" )]
		public virtual bool DoNotShowInOperation
		{
			get => _doNotShowInOperation;
			set => SetField( ref _doNotShowInOperation, value );
		}

		[Display( Name = "Страховка" )]
		public virtual Insurance Insurance
		{
			get => _insurance;
			set => SetField( ref _insurance, value );
		}

		[Display( Name = "Стоимость ремонта" )]
		public virtual decimal Price
		{
			get => _price;
			set => SetField( ref _price, value );
		}

		#endregion

		public override string ToString()
		{
			return $"Событие ТС №{Id} {CarEventType.Name}";
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(CarEventType == null)
			{
				yield return new ValidationResult("Вид события ТС должен быть указан.",
					new[] { nameof(CarEventType) });
			}

			if(Car == null)
			{
				yield return new ValidationResult("Автомобиль должен быть указан.",
					new[] { nameof(Car) });
			}

			if(StartDate == default(DateTime))
			{
				yield return new ValidationResult("Дата начала события должна быть указана.",
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

			if(CarEventType != null && CarEventType.NeedComment && string.IsNullOrEmpty(Comment))
			{
				yield return new ValidationResult("Комментарий должен быть заполнен.",
						new[] { nameof(Comment) });
			}

			if(Comment?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина комментария ({Comment.Length}/255).",
					new[] { nameof(Comment) });
			}
		}

		#endregion
	}

	public enum Insurance
	{
		None,
		Casco,
		Ssago,
		Gto
	}

	public class InsuranceOfUseStringType : EnumStringType
	{
		public InsuranceOfUseStringType() : base( typeof( Insurance ) )
		{ }
	}
}
