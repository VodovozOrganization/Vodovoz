using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class IncomeCategoryJournalRegistrar : ColumnsConfigRegistrarBase<IncomeCategoryJournalViewModel, IncomeCategoryJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<IncomeCategoryJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Уровень 1")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Level1)
					.XAlign(0.5f)
				.AddColumn("Уровень 2")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Level2)
					.XAlign(0.5f)
				.AddColumn("Уровень 3")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Level3)
					.XAlign(0.5f)
				.AddColumn("Уровень 4")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Level4)
					.XAlign(0.5f)
				.AddColumn("Уровень 5")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Level5)
					.XAlign(0.5f)
				.AddColumn("В архиве?")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(n => n.IsArchive)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Подразделение")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Subdivision)
					.XAlign(0.5f)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? GdkColors.DarkGrayColor : GdkColors.BlackColor)
				.Finish();
	}
}
