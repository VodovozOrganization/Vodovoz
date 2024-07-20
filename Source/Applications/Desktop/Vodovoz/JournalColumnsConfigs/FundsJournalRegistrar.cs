using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Organizations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Organizations;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FundsJournalRegistrar : ColumnsConfigRegistrarBase<FundsJournalViewModel, FundsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FundsJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();
	}
}
