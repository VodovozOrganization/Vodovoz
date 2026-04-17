using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	/// <summary>
	/// Массовая рассылка
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "массовая рассылка",
		Nominative = "массовая рассылка")]
	public class BulkEmail : CounterpartyEmail
	{
        private OrderDocument _orderDocument;
        public override IEmailableDocument EmailableDocument { get; }
		public override CounterpartyEmailType Type => CounterpartyEmailType.Bulk;

        /// <summary>
        /// Документ заказа
        /// </summary>
        [Display(Name = "Документ заказа")]
        public virtual OrderDocument OrderDocument
        {
            get => _orderDocument;
            set => SetField(ref _orderDocument, value);
        }
    }

	/// <summary>
	/// Общий счет по Email
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "общие счета",
		Nominative = "общий счет")]
	public class GeneralBillDocumentEmail : CounterpartyEmail
	{
        public override IEmailableDocument EmailableDocument { get; }
		public override CounterpartyEmailType Type => CounterpartyEmailType.GeneralBillDocument;
    }
}
