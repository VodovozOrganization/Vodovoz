using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на предоплату",
		Nominative = "счет без отгрузки на предоплату")]
	public class OrderWithoutShipmentForAdvancePaymentEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForAdvancePayment _orderWithoutShipmentForAdvancePayment;

		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForAdvancePayment;
		public override CounterpartyEmailType Type => CounterpartyEmailType.OrderWithoutShipmentForAdvancePayment;

		[Display(Name = "Счёт без отгрузки на предоплату")]
		public virtual OrderWithoutShipmentForAdvancePayment OrderWithoutShipmentForAdvancePayment
		{
			get => _orderWithoutShipmentForAdvancePayment;
			set => SetField(ref _orderWithoutShipmentForAdvancePayment, value);
		}
	}
}
