using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты приёма-передачи оборудования по email",
		Nominative = "акт приёма-передачи оборудования для отправки по email")]
	public class EquipmentTransferDocumentEmail : CounterpartyEmail
	{
		private OrderDocument _orderDocument;

		public override IEmailableDocument EmailableDocument => (IEmailableDocument) OrderDocument;
		public override CounterpartyEmailType Type => CounterpartyEmailType.BillDocument;

		[Display(Name = "Документ заказа")]
		public virtual OrderDocument OrderDocument
		{
			get => _orderDocument;
			set => SetField(ref _orderDocument, value);
		}
	}
}
