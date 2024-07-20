using Gamma.ColumnConfig;
using Vodovoz.Presentation.ViewModels.Organizations.Journals;

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
