using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на долги для отправки",
		Nominative = "счет без отгрузки на долги для отправки")]
	public class OrderWithoutShipmentForDebtEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForDebt _orderWithoutShipmentForDebt;
		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForDebt;

		[Display(Name = "Счёт без отгрузки на долг")]
		public virtual OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt
		{
			get => _orderWithoutShipmentForDebt;
			set => SetField(ref _orderWithoutShipmentForDebt, value);
		}
	}
}
