using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	public class OrderDocumentEmail : CounterpartyEmail
	{
		private OrderDocument _orderDocument;
		public override string CounterpartyFullName => OrderDocument.Order.Client.FullName;
		public override IEmailableDocument EmailableDocument => (IEmailableDocument) OrderDocument;

		[Display(Name = "Документ заказа для отправки")]
		public virtual OrderDocument OrderDocument
		{
			get => _orderDocument;
			set => SetField(ref _orderDocument, value);
		}
	}
}
