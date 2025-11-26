using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку документов заказа по ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку документов заказа по ЭДО",
		NominativePlural = "заявки на отправку документов заказа по ЭДО"
	)]
	public class OrderEdoRequest : CustomerEdoRequest
	{
		private OrderEntity _order;

		/// <summary>
		/// Код заказа
		/// </summary>
		[Display(Name = "Код заказа")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		public override EdoRequestType DocumentRequestType => EdoRequestType.Primary;

		// enum типа документа заказа
	}
}
