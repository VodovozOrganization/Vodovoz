using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "документы самовывоза",
		Nominative = "документ самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentEntity : PropertyChangedBase, IDomainObject
	{
		private OrderEntity _order;

		public virtual int Id { get; set; }

		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
	}
}
