using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EquipmentKindJournalRegistrar : ColumnsConfigRegistrarBase<EquipmentKindJournalViewModel, EquipmentKindJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EquipmentKindJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Гарантийный талон").AddTextRenderer(node => node.WarrantyCardType.GetEnumTitle())
				.Finish();
	}
}
