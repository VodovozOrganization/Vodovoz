using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	public class RequestsForCallClosedReasonsJournalRegistrar
		: ColumnsConfigRegistrarBase<RequestsForCallClosedReasonsJournalViewModel, RequestsForCallClosedReasonsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RequestsForCallClosedReasonsJournalNode> config) =>
			config
				.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Архивная").AddToggleRenderer(node => node.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
