using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class InventoryNomenclaturesJournalRegistrar :
		ColumnsConfigRegistrarBase<InventoryNomenclaturesJournalViewModel, NomenclatureJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureJournalNode> config) =>
			config
				.AddColumn("Код")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("Номенклатура")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Категория")
					.AddTextRenderer(node => node.AvailableText)
					.AddTextRenderer(node => node.Category.GetEnumTitle())
				.AddColumn("Код в ИМ")
					.AddTextRenderer(node => node.OnlineStoreExternalId)
				.Finish();
	}
}
