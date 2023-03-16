using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NomenclatureBalanceByStockJournalRegistrar : ColumnsConfigRegistrarBase<NomenclatureBalanceByStockJournalViewModel, NomenclatureBalanceByStockJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureBalanceByStockJournalNode> config) =>
			config.AddColumn("Склад").AddTextRenderer(node => node.WarehouseName)
				.AddColumn("Кол-во").AddTextRenderer(node => $"{node.NomenclatureAmount:N0}")
				.AddColumn("")
				.Finish();
	}
}
