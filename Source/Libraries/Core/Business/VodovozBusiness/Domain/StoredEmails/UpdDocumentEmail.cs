using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "УПД для отправки по email",
		Nominative = "УПД для отправки по email")]
	public class UpdDocumentEmail : CounterpartyEmail
	{
		private OrderDocument _orderDocument;
		public override IEmailableDocument EmailableDocument => (IEmailableDocument)OrderDocument;
		public override CounterpartyEmailType Type => CounterpartyEmailType.UpdDocument;

		[Display(Name = "Документ заказа")]
		public virtual OrderDocument OrderDocument
		{
			get => _orderDocument;
			set => SetField(ref _orderDocument, value);
		}
	}
}
