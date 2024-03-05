using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	public class OnlineOrderItem : Product, IDomainObject
	{
		private int? _nomenclatureId;
		private decimal _price;
		private bool _isDiscountInMoney;
		private decimal _percentDiscount;
		private decimal _moneyDiscount;
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
		
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}
		
		[Display(Name = "Скидка в деньгах")]
		public virtual bool IsDiscountInMoney
		{
			get => _isDiscountInMoney;
			set => SetField(ref _isDiscountInMoney, value);
		}
		
		[Display(Name = "Скидка в процентах")]
		public virtual decimal PercentDiscount
		{
			get => _percentDiscount;
			set => SetField(ref _percentDiscount, value);
		}
		
		[Display(Name = "Скидка в деньгах")]
		public virtual decimal MoneyDiscount
		{
			get => _moneyDiscount;
			set => SetField(ref _moneyDiscount, value);
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

		public virtual decimal Sum => Math.Round(Price * Count - MoneyDiscount, 2);

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
				MoneyDiscount = 0;
				PercentDiscount = 0;
				return;
			}
			
			if(IsDiscountInMoney)
			{
				MoneyDiscount = discount > Price * Count ? Price * Count : (discount < 0 ? 0 : discount);
				PercentDiscount = (100 * MoneyDiscount) / (Price * Count);
			}
			else
			{
				PercentDiscount = discount > 100 ? 100 : (discount < 0 ? 0 : discount);
				MoneyDiscount = Price * Count * PercentDiscount / 100;
			}
		}
	}
}
