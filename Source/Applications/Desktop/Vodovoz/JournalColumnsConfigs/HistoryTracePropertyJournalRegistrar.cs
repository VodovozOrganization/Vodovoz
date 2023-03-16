using Gamma.ColumnConfig;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.HistoryTrace;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class HistoryTracePropertyJournalRegistrar : ColumnsConfigRegistrarBase<HistoryTracePropertyJournalViewModel, HistoryTracePropertyNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<HistoryTracePropertyNode> config) =>
			config.AddColumn("Имя")
					.AddTextRenderer(node => node.PropertyName)
				.AddColumn("Тип")
					.AddTextRenderer(node => node.PropertyPath)
				.AddColumn("")
				.Finish();
	}
}
