using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using static Vodovoz.ViewModels.Journals.JournalViewModels.Logistic.RouteColumnJournalViewModel;

namespace Vodovoz.JournalColumnsConfigs
{
	public class RouteColumnJournalRegistrar : ColumnsConfigRegistrarBase<RouteColumnJournalViewModel, RouteColumnJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RouteColumnJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("Короткое название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.ShortName)
				.AddColumn("Выделить?")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(node => node.IsHighlighted)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();
	}
}
