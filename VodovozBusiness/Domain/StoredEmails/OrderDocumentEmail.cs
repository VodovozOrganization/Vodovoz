using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы заказа для отправки",
		Nominative = "документ заказа для отправки")]
	public class OrderDocumentEmail : CounterpartyEmail
	{
		private OrderDocument _orderDocument;
		public override IEmailableDocument EmailableDocument => (IEmailableDocument) OrderDocument;

		[Display(Name = "Документ заказа для отправки")]
		public virtual OrderDocument OrderDocument
		{
			get => _orderDocument;
			set => SetField(ref _orderDocument, value);
		}
	}
}
