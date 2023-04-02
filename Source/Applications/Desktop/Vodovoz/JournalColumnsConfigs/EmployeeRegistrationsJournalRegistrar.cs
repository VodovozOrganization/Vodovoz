using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EmployeeRegistrationsJournalRegistrar : ColumnsConfigRegistrarBase<EmployeeRegistrationsJournalViewModel, EmployeeRegistrationsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EmployeeRegistrationsJournalNode> config) =>
			config.AddColumn("Код")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("Оформление")
					.AddEnumRenderer(node => node.RegistrationType)
				.AddColumn("Форма оплаты")
					.AddEnumRenderer(node => node.PaymentForm)
				.AddColumn("Ставка налога")
					.AddNumericRenderer(node => node.TaxRate)
					.Digits(2)
				.AddColumn("")
				.Finish();
	}
}
