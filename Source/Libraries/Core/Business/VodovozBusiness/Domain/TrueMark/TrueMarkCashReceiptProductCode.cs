using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.TrueMark
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды честного знака для отправки чеков",
			Nominative = "код честного знака для отправки чеков"
		)
	]
	public class TrueMarkCashReceiptProductCode : PropertyChangedBase, IDomainObject
	{
		private TrueMarkCashReceiptOrder _trueMarkCashReceiptOrder;

		public int Id { get; set; }

		[Display(Name = "Заказ с честным знаком для отправки чека")]
		public virtual TrueMarkCashReceiptOrder TrueMarkCashReceiptOrder
		{
			get => _trueMarkCashReceiptOrder;
			set => SetField(ref _trueMarkCashReceiptOrder, value);
		}

		private OrderItem _orderItem;
		[Display(Name = "Строка заказа")]
		public virtual OrderItem OrderItem
		{
			get => _orderItem;
			set => SetField(ref _orderItem, value);
		}

	}
}
