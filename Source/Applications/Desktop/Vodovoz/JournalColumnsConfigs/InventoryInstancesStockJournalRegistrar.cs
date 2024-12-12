using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class InventoryInstancesStockJournalRegistrar :
		ColumnsConfigRegistrarBase<InventoryInstancesStockBalanceJournalViewModel, InventoryInstancesStockJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<InventoryInstancesStockJournalNode> config) =>
			config
				.AddColumn("Код")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("Инвентарный номер")
					.AddTextRenderer(node => node.GetInventoryNumber)
				.AddColumn("Код номенклатуры")
					.AddNumericRenderer(node => node.NomenclatureId)
				.AddColumn("Номенклатура")
					.AddTextRenderer(node => node.NomenclatureName)
				.AddColumn("Баланс")
					.AddNumericRenderer(node => node.Balance)
				.AddColumn("")
				.Finish();
	}
}
