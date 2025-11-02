using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
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
		private NomenclatureEntity _nomenclature;
		private int _quantity;
		private TrueMarkWaterIdentificationCode _individualCode;
		private TrueMarkWaterGroupCode _groupCode;

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
		public virtual TransferOrder TransferOrder
		{
			get => _transferOrder;
			set => SetField(ref _transferOrder, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Количество")]
		public virtual int Quantity
		{
			get => _quantity;
			set => SetField(ref _quantity, value);
		}

		[Display(Name = "Индивидуальный код")]
		public virtual TrueMarkWaterIdentificationCode IndividualCode
		{
			get => _individualCode;
			set => SetField(ref _individualCode, value);
		}

		[Display(Name = "Групповой код")]
		public virtual TrueMarkWaterGroupCode GroupCode
		{
			get => _groupCode;
			set => SetField(ref _groupCode, value);
		}
	}
}
