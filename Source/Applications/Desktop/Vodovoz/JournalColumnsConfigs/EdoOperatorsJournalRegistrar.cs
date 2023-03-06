using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EdoOperatorsJournalRegistrar : ColumnsConfigRegistrarBase<EdoOperatorsJournalViewModel, EdoOpeartorJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EdoOpeartorJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Брендовое название").AddNumericRenderer(node => node.BrandName)
				.AddColumn("Трёхзначный код").AddNumericRenderer(node => node.Code)
				.AddColumn("")
				.Finish();
	}
}
