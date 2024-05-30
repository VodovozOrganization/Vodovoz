using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "страховка авто",
		NominativePlural = "страховки авто",
		Genitive = "страховки авто",
		GenitivePlural = "страховок авто")]
	[HistoryTrace]
	public class CarInsurance : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _insuranceNumberMaxLength = 100;
		private const int _osagoNumberLength = 13;
		private const int _osagoNumberLettersCount = 3;
		private const int _osagoNumberDigitsCount = 10;

		private Car _car;
		private DateTime _startDate;
		private DateTime _endDate;
		private Counterparty _insurer;
		private string _insuranceNumber;
		private CarInsuranceType _insuranceType;

		public virtual int Id { get; set; }

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		[Display(Name = "Дата начала действия страховки")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Дата окончания действия страховки")]
		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Страховщик")]
		public virtual Counterparty Insurer
		{
			get => _insurer;
			set => SetField(ref _insurer, value);
		}

		[Display(Name = "Номер страховки")]
		public virtual string InsuranceNumber
		{
			get => _insuranceNumber;
			set => SetField(ref _insuranceNumber, value);
		}

		[Display(Name = "Тип страховки")]
		public virtual CarInsuranceType InsuranceType
		{
			get => _insuranceType;
			set => SetField(ref _insuranceType, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Car is null)
			{
				yield return new ValidationResult(
					"Автомобиль должен быть указан",
					new[] { nameof(Car) });
			}

			if(StartDate == default)
			{
				yield return new ValidationResult(
					"Дата начала действия страховки должна быть указана",
					new[] { nameof(StartDate) });
			}

			if(EndDate == default)
			{
				yield return new ValidationResult(
					"Дата окончания действия страховки должна быть указана",
					new[] { nameof(EndDate) });
			}

			if(Insurer is null)
			{
				yield return new ValidationResult(
					"Страховщик должен быть указан",
					new[] { nameof(Insurer) });
			}

			if(string.IsNullOrWhiteSpace(InsuranceNumber))
			{
				yield return new ValidationResult(
					"Номер страховки должен быть указан",
					new[] { nameof(InsuranceNumber) });
			}

			if(!string.IsNullOrWhiteSpace(InsuranceNumber))
			{
				if(InsuranceNumber.Length > _insuranceNumberMaxLength)
				{
					yield return new ValidationResult(
						$"Длина номера страховки не должна превышать {_insuranceNumberMaxLength} символов",
						new[] { nameof(InsuranceNumber) });
				}

				if(InsuranceType == CarInsuranceType.Osago)
				{
					if(InsuranceNumber.Length == _osagoNumberLength)
					{
						if(!InsuranceNumber.Substring(0, _osagoNumberLettersCount).All(char.IsLetter))
						{
							yield return new ValidationResult(
							$"Номер страховки по Осаго должен начинаться с {_osagoNumberLettersCount}-х букв",
							new[] { nameof(InsuranceNumber) });
						}

						if(!InsuranceNumber.Substring(_osagoNumberLettersCount, _osagoNumberDigitsCount).All(char.IsDigit))
						{
							yield return new ValidationResult(
							$"Номер страховки по Осаго должен заканчиваться {_osagoNumberDigitsCount}-ю цифрами",
							new[] { nameof(InsuranceNumber) });
						}
					}
					else
					{
						yield return new ValidationResult(
						$"Длина номера страховки по Осаго должна составлять {_osagoNumberLength} символов",
						new[] { nameof(InsuranceNumber) });
					}
				}
			}
		}
	}
}
