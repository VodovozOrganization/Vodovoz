using Gamma.ColumnConfig;
using Gamma.Utilities;
using Pango;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	public class OrdersRatingsJournalRegistrar
		: ColumnsConfigRegistrarBase<OrdersRatingsJournalViewModel, OrdersRatingsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrdersRatingsJournalNode> config) =>
			config
				.AddColumn(OrdersRatingsJournalNode.IdColumn).AddNumericRenderer(node => node.Id)
				.AddColumn(OrdersRatingsJournalNode.OnlineOrderIdColumn).AddTextRenderer(node => node.OnlineOrderId.ToString())
				.AddColumn(OrdersRatingsJournalNode.OrderIdColumn).AddTextRenderer(node => node.OrderId.ToString())
				.AddColumn(OrdersRatingsJournalNode.CreatedColumn).AddTextRenderer(node => node.OrderRatingCreated.ToString())
				.AddColumn(OrdersRatingsJournalNode.StatusColumn).AddTextRenderer(node => node.OrderRatingStatus.GetEnumTitle())
				.AddColumn(OrdersRatingsJournalNode.RatingColumn).AddNumericRenderer(node => node.Rating)
				.AddColumn(OrdersRatingsJournalNode.ReasonsColumn).AddTextRenderer(node => node.OrderRatingReasons)
				.AddColumn(OrdersRatingsJournalNode.CommentColumn).AddTextRenderer(node => node.OrderRatingComment)
					.WrapMode(WrapMode.Word)
					.WrapWidth(400)
				.AddColumn(OrdersRatingsJournalNode.SourceColumn).AddTextRenderer(node => node.OrderRatingSource.GetEnumTitle())
				.AddColumn("")
				.Finish();
	}
}
