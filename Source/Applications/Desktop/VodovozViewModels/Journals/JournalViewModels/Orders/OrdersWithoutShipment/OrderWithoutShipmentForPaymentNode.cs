using QS.DomainModel.Entity;
using QS.Project.Journal;
using System;
using Vodovoz.Domain.Orders;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForPaymentNode : JournalEntityNodeBase<VodOrder>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public bool IsSelected { get; set; }
		public int OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
		public decimal Bottles { get; set; }
		public decimal OrderSum { get; set; }
		public string DeliveryAddress { get; set; }
		public string OrganizationName { get; set; }
	}
}
