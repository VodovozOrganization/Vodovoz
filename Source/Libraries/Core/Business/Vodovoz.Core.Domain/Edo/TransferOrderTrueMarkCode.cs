using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Код ЧЗ заказа по перемещению подотчетных в ЧЗ товаров между организациями
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "коды ЧЗ заказов по перемещению товаров между организациями",
		Nominative = "код ЧЗ по перемещению товара между организациями")]
	public class TransferOrderTrueMarkCode : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private TransferOrder _transferOrder;
		private TrueMarkWaterIdentificationCode _trueMarkCode;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Заказ по перемещению товаров между организациями
		/// </summary>
		[Display(Name = "Заказ по перемещению товаров")]
		public TransferOrder TransferOrder
		{
			get => _transferOrder;
			set => SetField(ref _transferOrder, value);
		}

		/// <summary>
		/// Код ЧЗ
		/// </summary>
		[Display(Name = "Код ЧЗ")]
		public TrueMarkWaterIdentificationCode TrueMarkCode
		{
			get => _trueMarkCode;
			set => SetField(ref _trueMarkCode, value);
		}
	}
}
