using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "оформление сотрудника",
		NominativePlural = "оформления сотрудников")]
	public class EmployeeRegistration : PropertyChangedBase, IDomainObject, IEmployeeRegistration
	{
		private RegistrationType _registrationType;
		private PaymentForm _paymentForm;
		private decimal _taxRate;

		public virtual int Id { get; set; }

		[Display(Name = "Вид оформления")]
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
			$"Вид оформления: {RegistrationType}, форма оплаты: {PaymentForm.GetEnumTitle()} ставка налога: {TaxRate}";
	}

	public enum PaymentForm
	{
		[Display(Name = "Нал")]
		Cash,
		[Display(Name = "Безнал")]
		Cashless
	}
	
	/*public enum RegistrationType
	{
		[Display(Name = "Самозанятый")]
		SelfEmployed,
		[Display(Name = "ИП")]
		PrivateBusinessman,
		[Display(Name = "ГПК")]
		Contract,
		[Display(Name = "ТК РФ")]
		LaborCode
	}*/
}
