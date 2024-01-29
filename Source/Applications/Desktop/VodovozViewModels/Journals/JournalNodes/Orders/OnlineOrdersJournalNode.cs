using System;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OnlineOrdersJournalNode : JournalEntityNodeBase
	{
		public override string Title => string.Empty;

		public string CounterpartyName { get; set; }
		public string CompiledAddress { get; set; }
		public DateTime DeliveryDate { get; set; }
		public int DeliveryScheduleId { get; set; }
		public OnlineOrderStatus OnlineOrderStatus { get; set; }
		public string ManagerWorkWith { get; set; }
		public Domain.Client.Source Source { get; set; }
		public decimal OnlineOrderSum { get; set; }
		public OnlineOrderPaymentStatus OnlineOrderPaymentStatus { get; set; }
		public int? OnlinePayment { get; set; }
		public OnlineOrderPaymentType OnlineOrderPaymentType { get; set; }
		public bool FastDelivery { get; set; }
		public bool IsNeedConfirmationByCall { get; set; }
		public int? OrderId { get; set; }
	}
}
