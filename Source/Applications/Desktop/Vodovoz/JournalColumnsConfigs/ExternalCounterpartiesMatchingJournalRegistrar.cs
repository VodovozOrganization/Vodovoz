using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ExternalCounterpartiesMatchingJournalRegistrar :
		ColumnsConfigRegistrarBase<ExternalCounterpartiesMatchingJournalViewModel, ExternalCounterpartyMatchingJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ExternalCounterpartyMatchingJournalNode> config) =>
			config
				.AddColumn("Телефон")
					.AddTextRenderer(node => node.PhoneNumber)
				.AddColumn("Источник")
					.AddEnumRenderer(node => node.CounterpartyFrom)
				.AddColumn("Дата/время")
					.AddTextRenderer(node => node.Created.ToString("g"))
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.Status)
				.AddColumn("Присвоенный КА")
					.AddTextRenderer(node => node.CounterpartyName)
				.AddColumn("")
				.Finish();

	}
}
