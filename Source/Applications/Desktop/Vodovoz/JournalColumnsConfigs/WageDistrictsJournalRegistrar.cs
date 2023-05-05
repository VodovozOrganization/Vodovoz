using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.WageCalculation;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class WageDistrictsJournalRegistrar : ColumnsConfigRegistrarBase<WageDistrictsJournalViewModel, WageDistrictJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WageDistrictJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("В архиве?")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsArchiveString)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish();
	}
}
