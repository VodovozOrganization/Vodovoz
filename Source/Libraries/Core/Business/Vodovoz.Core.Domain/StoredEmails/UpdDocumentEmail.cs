using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Core.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "УПД для отправки по email",
		Nominative = "УПД для отправки по email")]
	public class UpdDocumentEmail : CounterpartyEmail
	{
		private OrderDocumentEntity _orderDocument;
		public override IEmailableDocument EmailableDocument => (IEmailableDocument)OrderDocument;
		public override CounterpartyEmailType Type => CounterpartyEmailType.UpdDocument;

		[Display(Name = "Документ заказа")]
		public virtual OrderDocumentEntity OrderDocument
		{
			get => _orderDocument;
			set => SetField(ref _orderDocument, value);
		}
	}
}
