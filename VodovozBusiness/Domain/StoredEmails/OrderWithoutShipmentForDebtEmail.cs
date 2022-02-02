using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	public class OrderWithoutShipmentForDebtEmail : CounterpartyEmail
	{
		private OrderWithoutShipmentForDebt _orderWithoutShipmentForDebt;
		public override string CounterpartyFullName => OrderWithoutShipmentForDebt.Client.FullName;
		public override IEmailableDocument EmailableDocument => OrderWithoutShipmentForDebt;

		[Display(Name = "Счёт без отгрузки на долг")]
		public virtual OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt
		{
			get => _orderWithoutShipmentForDebt;
			set => SetField(ref _orderWithoutShipmentForDebt, value);
		}
	}
}
