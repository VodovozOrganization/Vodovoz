using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на предоплату для отправки",
		Nominative = "счет без отгрузки на предоплату для отправки")]
	public class OrderWithoutShipmentForAdvancePaymentEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForAdvancePayment _orderWithoutShipmentForAdvancePayment;
		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForAdvancePayment;

		[Display(Name = "Счёт без отгрузки на предоплату")]
		public virtual OrderWithoutShipmentForAdvancePayment OrderWithoutShipmentForAdvancePayment
		{
			get => _orderWithoutShipmentForAdvancePayment;
			set => SetField(ref _orderWithoutShipmentForAdvancePayment, value);
		}
	}
}
