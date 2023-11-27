using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Counterparties;
using static Vodovoz.ViewModels.Counterparties.TagJournalViewModel;

namespace Vodovoz.JournalColumnsConfigs
{
	internal class TagJournalRegistrar : ColumnsConfigRegistrarBase<TagJournalViewModel, TagJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<TagJournalNode> config) =>
			config.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Цвет").AddTextRenderer()
				.AddSetter((cell, node) => { cell.Markup = $"<span foreground=\"{node.ColorText}\">♥</span>"; })
				.Finish();
	}
}
