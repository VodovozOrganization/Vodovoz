using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Документ самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "документы самовывоза",
		Nominative = "документ самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentEntity : PropertyChangedBase, IDomainObject
	{
		private OrderEntity _order;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Заказ, к которому относится документ самовывоза
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
	}
}
