using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	/// <summary>
	/// Код ЧЗ товара строки заказа
	/// </summary>
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров строк заказов",
			Nominative = "код ЧЗ товара строки заказа")]
	[HistoryTrace]
	public class TrueMarkProductCodeOrderItem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _trueMarkProductCodeId;
		private int _orderItemId;

		/// <summary>
		/// Id
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Id кода ЧЗ товара
		/// </summary>
		public virtual int TrueMarkProductCodeId
		{
			get => _trueMarkProductCodeId;
			set => SetField(ref _trueMarkProductCodeId, value);
		}

		/// <summary>
		/// Id строки заказа
		/// </summary>
		public virtual int OrderItemId
		{
			get => _orderItemId;
			set => SetField(ref _orderItemId, value);
		}
	}
}
