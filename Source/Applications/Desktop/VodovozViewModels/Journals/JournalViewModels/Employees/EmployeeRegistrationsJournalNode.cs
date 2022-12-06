using Gamma.Utilities;
using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class EmployeeRegistrationsJournalNode : JournalNodeBase
	{
		public int Id { get; set; }
		public RegistrationType RegistrationType { get; set; }
		public PaymentForm PaymentForm { get; set; }
		public decimal TaxRate { get; set; }
		public override string Title =>
			$"Оформление: {RegistrationType.GetEnumTitle()}, форма оплаты: {PaymentForm.GetEnumTitle()} ставка налога: {TaxRate}";
	}
}
