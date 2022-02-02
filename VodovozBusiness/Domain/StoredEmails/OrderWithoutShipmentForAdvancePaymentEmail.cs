using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	public class OrderWithoutShipmentForAdvancePaymentEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForAdvancePayment _orderWithoutShipmentForAdvancePayment;
		public override string CounterpartyFullName => OrderWithoutShipmentForAdvancePayment.Client.FullName;
		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForAdvancePayment;

		[Display(Name = "Счёт без отгрузки на предоплату")]
		public virtual OrderWithoutShipmentForAdvancePayment OrderWithoutShipmentForAdvancePayment
		{
			get => _orderWithoutShipmentForAdvancePayment;
			set => SetField(ref _orderWithoutShipmentForAdvancePayment, value);
		}
	}
}
