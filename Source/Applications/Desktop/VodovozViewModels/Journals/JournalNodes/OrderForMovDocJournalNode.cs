using QS.DomainModel.Entity;
using QS.Project.Journal;
using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Journals.JournalNodes
{
	public class OrderForMovDocJournalNode : JournalEntityNodeBase<Order>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";

		public OrderStatus StatusEnum { get; set; }

		public DateTime Date { get; set; }
		public DateTime CreateDate { get; set; }
		public bool IsSelfDelivery { get; set; }
		public decimal BottleAmount { get; set; }

		public string Counterparty { get; set; }

		public decimal Sum { get; set; }

		public string CompilledAddress { get; set; }
		public string Address => IsSelfDelivery ? "Самовывоз" : CompilledAddress;

		public int? OnlineOrder { get; set; }
		public string OnLineNumber => OnlineOrder?.ToString() ?? string.Empty;

		public int? EShopOrder { get; set; }
		public string EShopNumber => EShopOrder?.ToString() ?? string.Empty;
	}
}
