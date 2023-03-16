using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PhonesJournalRegistrar : ColumnsConfigRegistrarBase<PhonesJournalViewModel, PhonesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PhonesJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Phone)
				.AddColumn("Тип").AddTextRenderer(node => node.Type)
				.AddColumn("")
				.Finish();
	}
}
