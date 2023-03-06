using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class WarehouseJournalRegistrar : ColumnsConfigRegistrarBase<WarehouseJournalViewModel, WarehouseJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WarehouseJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("")
				.Finish();
	}
}
