using System;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OrdersRatingsJournalNode : JournalEntityNodeBase
	{
		public override string Title => string.Empty;
		
		public int? OnlineOrderId { get; set; }
		public int? OrderId { get; set; }
		public DateTime OrderRatingCreated { get; set; }
		public OrderRatingStatus OrderRatingStatus { get; set; }
		public int Rating { get; set; }
		public string OrderRatingReasons { get; set; }
		public string Employee { get; set; }
		public string OrderRatingComment { get; set; }
		public Core.Domain.Clients.Source OrderRatingSource { get; set; }
	}
}
