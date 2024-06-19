using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	public class OnlineOrderCancellationReasonsJournalRegistrar
		: ColumnsConfigRegistrarBase<OnlineOrderCancellationReasonsJournalViewModel, OnlineOrderCancellationReasonsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OnlineOrderCancellationReasonsJournalNode> config) =>
			config
				.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Архивная").AddToggleRenderer(node => node.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
