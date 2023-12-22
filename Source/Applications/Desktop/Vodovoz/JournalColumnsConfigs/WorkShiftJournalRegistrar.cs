using Gamma.ColumnConfig;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Presentation.ViewModels.Pacs.Journals;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class WorkShiftJournalRegistrar : ColumnsConfigRegistrarBase<WorkShiftJournalViewModel, WorkShift>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WorkShift> config) =>
			config
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Длительность смены").AddTextRenderer(node => node.Duration.ToString("hh\\:mm\\:ss"))
				.AddColumn("")
				.Finish();
	}
}
