using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "вид оформления сотрудника",
		NominativePlural = "виды оформлений сотрудников")]
	[HistoryTrace]
	[EntityPermission]
	public class EmployeeRegistration : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private RegistrationType _registrationType;
		private PaymentForm _paymentForm;
		private decimal _taxRate;

		public virtual int Id { get; set; }

		[Display(Name = "Оформление")]
		public virtual RegistrationType RegistrationType
		{
			get => _registrationType;
			set => SetField(ref _registrationType, value);
		}

		[Display(Name = "Форма оплаты")]
		public virtual PaymentForm PaymentForm
		{
			get => _paymentForm;
			set => SetField(ref _paymentForm, value);
		}

		[Display(Name = "Ставка налога")]
		public virtual decimal TaxRate
		{
			get => _taxRate;
			set => SetField(ref _taxRate, value);
		}

		public override string ToString() =>
			$"Оформление: {RegistrationType.GetEnumTitle()}, форма оплаты: {PaymentForm.GetEnumTitle()}, ставка налога: {TaxRate}%";

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.GetService(typeof(IEmployeeRepository)) is IEmployeeRepository employeeRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(employeeRepository) }");
			}
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			var duplicate = employeeRepository.EmployeeRegistrationDuplicateExists(uowFactory, this);
			if(duplicate != null)
			{
				yield return new ValidationResult(
					$"Вид оформления с такими параметрами уже существует(Код {duplicate.Id}).\n" +
					"Выберите сохраненный или смените параметры");
			}
		}
	}

	public enum PaymentForm
	{
		[Display(Name = "Нал")]
		Cash,
		[Display(Name = "Безнал")]
		Cashless
	}
	
	public enum RegistrationType
	{
		[Display(Name = "Самозанятый")]
		SelfEmployed,
		[Display(Name = "ИП")]
		PrivateBusinessman,
		[Display(Name = "ГПХ")]
		Contract,
		[Display(Name = "ТК РФ")]
		LaborCode
	}
}
