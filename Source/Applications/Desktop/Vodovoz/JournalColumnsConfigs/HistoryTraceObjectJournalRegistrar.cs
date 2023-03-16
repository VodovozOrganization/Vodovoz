using Gamma.ColumnConfig;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.HistoryTrace;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class HistoryTraceObjectJournalRegistrar : ColumnsConfigRegistrarBase<HistoryTraceObjectJournalViewModel, HistoryTraceObjectNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<HistoryTraceObjectNode> config) =>
			config.AddColumn("Имя")
					.AddTextRenderer(node => node.DisplayName)
				.AddColumn("Тип")
					.AddTextRenderer(node => node.ObjectType.ToString())
				.AddColumn("")
				.Finish();
	}
}
