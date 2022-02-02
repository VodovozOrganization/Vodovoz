using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	public class BulkEmail : CounterpartyEmail
	{
		private Counterparty _counterparty;
		public override string CounterpartyFullName { get; }
		public override IEmailableDocument EmailableDocument { get; }

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
	}
}
