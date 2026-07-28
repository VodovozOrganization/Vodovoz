using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace VodovozBusiness.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "письма о закрытии поставок",
		Nominative = "письмо о закрытии поставок")]
	public class ClosingDeliveriesEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForDebt _orderWithoutShipmentForDebt;

		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForDebt;
		public override CounterpartyEmailType Type => CounterpartyEmailType.ClosingDeliveries;

		[Display(Name = "Счёт без отгрузки на долг")]
		public virtual OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt
		{
			get => _orderWithoutShipmentForDebt;
			set => SetField(ref _orderWithoutShipmentForDebt, value);
		}
	}
}
