using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Banks;
using Vodovoz.ViewModels.Journals.JournalViewModels.Banks;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class BanksJournalRegistrar : ColumnsConfigRegistrarBase<BanksJournalViewModel, BanksJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<BanksJournalNode> config) =>
			config
				.AddColumn("Код")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("БИК")
					.AddTextRenderer(node => node.Bik)
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Город")
					.AddTextRenderer(node => node.City)
				.AddColumn("")
				.Finish();
	}
}
