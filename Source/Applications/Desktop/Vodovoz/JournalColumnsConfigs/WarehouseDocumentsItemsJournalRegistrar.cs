using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class WarehouseDocumentsItemsJournalRegistrar : ColumnsConfigRegistrarBase<WarehouseDocumentsItemsJournalViewModel, WarehouseDocumentsItemsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WarehouseDocumentsItemsJournalNode> config) =>
			config.AddColumn("#").AddNumericRenderer(node => node.Id)
				.AddColumn("Тип").AddTextRenderer(node => node.EntityType.Name)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(100)
				.AddColumn("").Finish();
	}
}
