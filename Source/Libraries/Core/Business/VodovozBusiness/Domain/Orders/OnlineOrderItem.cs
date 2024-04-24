using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки онлайн заказа",
		Nominative = "строка онлайн заказа")]
	public class OnlineOrderItem : Product, IDomainObject
	{
		private int? _nomenclatureId;
		private int? _promoSetId;
		private OnlineOrder _onlineOrder;

		protected OnlineOrderItem() { } 

		public virtual int Id { get; set; }
		
		[Display(Name = "Онлайн заказ")]
		public virtual OnlineOrder OnlineOrder
		{
			get => _onlineOrder;
			set => SetField(ref _onlineOrder, value);
		}
		
		[Display(Name = "Id номенклатуры")]
		public virtual int? NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}
		
		[Display(Name = "Id промонабора")]
		public virtual int? PromoSetId
		{
			get => _promoSetId;
			set => SetField(ref _promoSetId, value);
		}
		
		[Display(Name = "Количество из промонабора")]
		public virtual decimal CountFromPromoSet { get; set; }
		
		[Display(Name = "Цена в ДВ")]
		public virtual decimal NomenclaturePrice { get; set; }
		
		[Display(Name = "Скидка из промонабора")]
		public virtual decimal DiscountFromPromoSet { get; set; }
		
		[Display(Name = "Тип скидки из промонабора")]
		public virtual bool IsDiscountInMoneyFromPromoSet { get; set; }
		
		public static OnlineOrderItem Create(
			int? nomenclatureId,
			decimal count,
			bool isDiscountInMoney,
			decimal discount,
			decimal price,
			int? promoSetId,
			Nomenclature nomenclature,
			PromotionalSet promotionalSet,
			OnlineOrder onlineOrder
		)
		{
			var onlineOrderItem = new OnlineOrderItem
			{
				NomenclatureId = nomenclatureId,
				Count = count,
				IsDiscountInMoney = isDiscountInMoney,
				Price = price,
				PromoSetId = promoSetId,
				Nomenclature = nomenclature,
				PromoSet = promotionalSet,
				OnlineOrder = onlineOrder
			};

			onlineOrderItem.CalculateDiscount(discount);

			return onlineOrderItem;
		}

		private void CalculateDiscount(decimal discount)
		{
			if(Price * Count == 0)
			{
				DiscountMoney = 0;
				Discount = 0;
				return;
			}
			
			if(IsDiscountInMoney)
			{
				DiscountMoney = discount > Price * Count ? Price * Count : (discount < 0 ? 0 : discount);
				Discount = (100 * DiscountMoney) / (Price * Count);
			}
			else
			{
				Discount = discount > 100 ? 100 : (discount < 0 ? 0 : discount);
				DiscountMoney = Price * Count * Discount / 100;
			}
		}
	}
}
