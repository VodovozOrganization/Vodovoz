using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	public class OrdersRatingReasonsJournalRegistrar
		: ColumnsConfigRegistrarBase<OrdersRatingReasonsJournalViewModel, OrdersRatingReasonsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrdersRatingReasonsJournalNode> config) =>
			config
				.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Оценки").AddTextRenderer(node => node.AvailableForRatings)
				.AddColumn("Архивная").AddToggleRenderer(node => node.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
