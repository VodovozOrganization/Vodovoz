using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class SalesChannelJournalRegistrar : ColumnsConfigRegistrarBase<SalesChannelJournalViewModel, SalesChannelJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<SalesChannelJournalNode> config) =>
			config.AddColumn("Номер")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Имя")
					.AddTextRenderer(node => node.Title)
				.AddColumn("")
				.Finish();
	}
}
