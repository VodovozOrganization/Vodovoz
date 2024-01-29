using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	public class OnlineOrderItem : PropertyChangedBase, IDomainObject
	{
		private int _nomenclatureId;
		private Nomenclature _nomenclature;
		private decimal _price;
		private decimal _count;
		private bool _isDiscountInMoney;
		private decimal _discount;
		private int _discountReasonId;
		private DiscountReason _discountReason;
		private int _promoSetId;
		private PromotionalSet _promoSet;
		private OnlineOrder _onlineOrder;

		public virtual int Id { get; set; }
		
		[Display(Name = "Онлайн заказ")]
		public virtual OnlineOrder OnlineOrder
		{
			get => _onlineOrder;
			set => SetField(ref _onlineOrder, value);
		}
		
		[Display(Name = "Id номенклатуры")]
		public virtual int NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}
		
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
		
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}
		
		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			set => SetField(ref _count, value);
		}
		
		[Display(Name = "Скидка в деньгах")]
		public virtual bool IsDiscountInMoney
		{
			get => _isDiscountInMoney;
			set => SetField(ref _isDiscountInMoney, value);
		}
		
		[Display(Name = "Скидка")]
		public virtual decimal Discount
		{
			get => _discount;
			set => SetField(ref _discount, value);
		}
		
		[Display(Name = "Id основания скидки")]
		public virtual int DiscountReasonId
		{
			get => _discountReasonId;
			set => SetField(ref _discountReasonId, value);
		}
		
		[Display(Name = "Основание скидки")]
		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}
		
		[Display(Name = "Id промонабора")]
		public virtual int PromoSetId
		{
			get => _promoSetId;
			set => SetField(ref _promoSetId, value);
		}
		
		[Display(Name = "Промонабор")]
		public virtual PromotionalSet PromoSet
		{
			get => _promoSet;
			set => SetField(ref _promoSet, value);
		}
	}
}
