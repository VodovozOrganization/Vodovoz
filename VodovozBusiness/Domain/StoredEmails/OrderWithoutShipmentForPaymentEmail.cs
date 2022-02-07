using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на постоплату",
		Nominative = "счет без отгрузки на постоплату")]
	public class OrderWithoutShipmentForPaymentEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForPayment _orderWithoutShipmentForPayment;

		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForPayment;
		public override CounterpartyEmailType Type => CounterpartyEmailType.OrderWithoutShipmentForPayment;

		[Display(Name = "Счёт без отгрузки на постоплату")]
		public virtual OrderWithoutShipmentForPayment OrderWithoutShipmentForPayment
		{
			get => _orderWithoutShipmentForPayment;
			set => SetField(ref _orderWithoutShipmentForPayment, value);
		}
	}
}
