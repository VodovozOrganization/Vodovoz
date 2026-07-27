using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// Массовая рассылка
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "массовая рассылка",
		Nominative = "массовая рассылка")]
	public class BulkEmail : CounterpartyEmail
	{
        private OrderDocumentEntity _orderDocument;
        public override IEmailableDocument EmailableDocument { get; }
		public override CounterpartyEmailType Type => CounterpartyEmailType.Bulk;

        /// <summary>
        /// Документ заказа
        /// </summary>
        [Display(Name = "Документ заказа")]
        public virtual OrderDocumentEntity OrderDocument
        {
            get => _orderDocument;
            set => SetField(ref _orderDocument, value);
        }
    }
}
