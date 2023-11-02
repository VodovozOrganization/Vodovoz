using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.WageCalculation;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class SalesPlanJournalRegistrar : ColumnsConfigRegistrarBase<SalesPlanJournalViewModel, SalesPlanJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<SalesPlanJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("Описание")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Title)
				.AddColumn("В архиве?")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsArchiveString)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
					{
						c.ForegroundGdk = n.IsArchive ? GdkColors.InsensitiveText : GdkColors.PrimaryText;
					})
				.Finish();
	}
}
