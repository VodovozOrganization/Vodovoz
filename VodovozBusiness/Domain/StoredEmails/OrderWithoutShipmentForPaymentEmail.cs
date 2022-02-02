using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	public class OrderWithoutShipmentForPaymentEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForPayment _orderWithoutShipmentForPayment;
		public override string CounterpartyFullName => OrderWithoutShipmentForPayment.Client.FullName;
		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForPayment;

		[Display(Name = "Счёт без отгрузки на постоплату")]
		public virtual OrderWithoutShipmentForPayment OrderWithoutShipmentForPayment
		{
			get => _orderWithoutShipmentForPayment;
			set => SetField(ref _orderWithoutShipmentForPayment, value);
		}
	}
}
