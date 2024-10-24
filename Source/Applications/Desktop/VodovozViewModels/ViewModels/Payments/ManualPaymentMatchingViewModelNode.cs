using System;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class ManualPaymentMatchingViewModelNode : JournalEntityNodeBase<Order>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public OrderStatus OrderStatus { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal ActualOrderSum { get; set; }
		public decimal LastPayments { get; set; }
		public decimal OldCurrentPayment { get; set; }
		public decimal CurrentPayment { get; set; }
		public bool Calculate { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
		public bool IsClosingDocumentsOrder { get; set; }
	}
}
