using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class GeoGroupJournalRegistrar : ColumnsConfigRegistrarBase<GeoGroupJournalViewModel, GeoGroupJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<GeoGroupJournalNode> config) =>
			config.AddColumn("№")
					.AddNumericRenderer(node => node.Id).WidthChars(4)
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.Finish();
	}
}
