using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Строки онлайн заказа",
		Nominative = "Строка онлайн заказа",
		Prepositional = "Строке онлайн заказа",
		PrepositionalPlural = "Строках онлайн заказа"
	)]
	[HistoryTrace]
	public class OnlineOrderItem : PropertyChangedBase, IDomainObject, IProduct
	{
		private int? _nomenclatureId;
		private decimal _price;
		private bool _isDiscountInMoney;
		private bool _isFixedPrice;
		private decimal _percentDiscount;
		private decimal _moneyDiscount;
		private int? _promoSetId;
		private OnlineOrder _onlineOrder;
		private decimal _count = -1;
		private DiscountReason _discountReason;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;

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
		
		[Display(Name = "Фикса")]
		public virtual bool IsFixedPrice
		{
			get => _isFixedPrice;
			set => SetField(ref _isFixedPrice, value);
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

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			protected set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Добавлено из промонабора")]
		public virtual PromotionalSet PromoSet
		{
			get => _promoSet;
			set => SetField(ref _promoSet, value);
		}

		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			protected set => SetField(ref _count, value);
		}

		[Display(Name = "Основание скидки на товар")]
		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}

		[Display(Name = "Количество из промонабора")]
		public virtual decimal CountFromPromoSet { get; set; }
		
		[Display(Name = "Цена в ДВ")]
		public virtual decimal NomenclaturePrice { get; set; }
		
		[Display(Name = "Скидка из промонабора")]
		public virtual decimal DiscountFromPromoSet { get; set; }
		
		[Display(Name = "Тип скидки из промонабора")]
		public virtual bool IsDiscountInMoneyFromPromoSet { get; set; }
		
		[Display(Name = "Тип ошибки валидации онлайн товара")]
		public virtual OnlineOrderErrorState? OnlineOrderErrorState { get; set; }

		public virtual decimal GetDiscount => IsDiscountInMoney ? MoneyDiscount : PercentDiscount;
		public virtual decimal Sum => Math.Round(Price * Count - MoneyDiscount, 2);
		public virtual decimal ActualSum => Sum;
		public virtual decimal CurrentCount => Count;
		
		public static OnlineOrderItem Create(
			int? nomenclatureId,
			decimal count,
			bool isDiscountInMoney,
			bool isFixedPrice,
			decimal discount,
			decimal price,
			int? promoSetId,
			DiscountReason discountReason,
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
				IsFixedPrice = isFixedPrice,
				Price = price,
				PromoSetId = promoSetId,
				DiscountReason = discountReason,
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
