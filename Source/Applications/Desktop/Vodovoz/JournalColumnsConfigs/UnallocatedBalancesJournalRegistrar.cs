using Gamma.ColumnConfig;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class UnallocatedBalancesJournalRegistrar : ColumnsConfigRegistrarBase<UnallocatedBalancesJournalViewModel, UnallocatedBalancesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<UnallocatedBalancesJournalNode> config) =>
			config.AddColumn("Код клиента")
					.AddNumericRenderer(node => node.CounterpartyId)
				.AddColumn("ИНН")
					.AddTextRenderer(node => node.CounterpartyINN)
				.AddColumn("Наименование")
					.AddTextRenderer(node => node.CounterpartyName)
				.AddColumn("Наша организация")
					.AddTextRenderer(node => node.OrganizationName)
				.AddColumn("Баланс клиента")
					.AddNumericRenderer(node => node.CounterpartyBalance)
					.Digits(2)
				.AddColumn("Долг клиента")
					.AddNumericRenderer(node => node.CounterpartyDebt)
					.Digits(2)
				.Finish();
	}
}
