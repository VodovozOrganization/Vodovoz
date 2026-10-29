using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на долги",
		Nominative = "счет без отгрузки на долги")]
	public class OrderWithoutShipmentForDebtEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForDebt _orderWithoutShipmentForDebt;

		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForDebt;
		public override CounterpartyEmailType Type => CounterpartyEmailType.OrderWithoutShipmentForDebt;

		[Display(Name = "Счёт без отгрузки на долг")]
		public virtual OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt
		{
			get => _orderWithoutShipmentForDebt;
			set => SetField(ref _orderWithoutShipmentForDebt, value);
		}
	}
}
