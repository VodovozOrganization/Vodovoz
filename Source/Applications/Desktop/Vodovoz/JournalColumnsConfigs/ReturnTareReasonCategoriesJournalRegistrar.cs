using Gamma.ColumnConfig;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ReturnTareReasonCategoriesJournalRegistrar : ColumnsConfigRegistrarBase<ReturnTareReasonCategoriesJournalViewModel, ReturnTareReasonCategoriesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ReturnTareReasonCategoriesJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Категория причины")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();
	}
}
