using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NonReturnReasonJournalRegistrar : ColumnsConfigRegistrarBase<NonReturnReasonJournalViewModel, NonReturnReasonJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NonReturnReasonJournalNode> config) =>
			config
				.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Необходимо начислять неустойку").AddToggleRenderer(node => node.NeedForfeit)
				.Editing(false)
				.XAlign(0f)
				.AddColumn("")
				.Finish();
	}
}
