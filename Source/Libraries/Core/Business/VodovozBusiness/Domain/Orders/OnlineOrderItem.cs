using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;

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
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;
		private IObservableList<DiscountReason> _discountReasons = new ObservableList<DiscountReason>();
		private decimal _discountPercentFromDiscountReasons;
		private decimal _discountMoneyFromDiscountReasons;
		private bool _isDiscountInMoneyFromDiscountReasons;

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

		/// <summary>
		/// Основания скидок на товар
		/// </summary>
		[Display(Name = "Основания скидки на товар")]
		public virtual IObservableList<DiscountReason> DiscountReasons
		{
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}

		/// <summary>
		/// Сумма скидок из всех полученных оснований, приведенная к деньгам
		/// </summary>
		[Display(Name = "Сумма скидок из всех полученных оснований, приведенная к деньгам")]
		public virtual decimal DiscountMoneyFromDiscountReasons
		{
			get => _discountMoneyFromDiscountReasons;
			set => SetField(ref _discountMoneyFromDiscountReasons, value);
		}

		/// <summary>
		/// Сумма скидок из всех полученных оснований, приведенная к процентам
		/// </summary>
		[Display(Name = "Сумма скидок из всех полученных оснований, приведенная к процентам")]
		public virtual decimal DiscountPercentFromDiscountReasons
		{
			get => _discountPercentFromDiscountReasons;
			set => SetField(ref _discountPercentFromDiscountReasons, value);
		}

		/// <summary>
		/// Есть ли среди оснований скидки на товар те, которые имеют тип скидки в деньгах
		/// </summary>
		[Display(Name = "Есть ли среди оснований скидки на товар те, которые имеют тип скидки в деньгах")]
		public virtual bool IsDiscountInMoneyFromDiscountReasons
		{
			get => _isDiscountInMoneyFromDiscountReasons;
			set => SetField(ref _isDiscountInMoneyFromDiscountReasons, value);
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

		/// <summary>
		/// Суммарное значение скидок из всех оснований скидки на товар
		/// Если среди скидок на товар были скидки в деньгах, то все скидки приводятся к деньгам и суммируются
		/// Если все скидки в процентах, то суммируются все проценты
		/// </summary>
		public virtual decimal GetDiscountFromDiscountReasons => IsDiscountInMoneyFromDiscountReasons ? DiscountMoneyFromDiscountReasons : DiscountPercentFromDiscountReasons;
		public virtual decimal Sum => Math.Round(Price * Count - MoneyDiscount, 2);
		public virtual decimal ActualSum => Sum;
		public virtual decimal CurrentCount => Count;

		/// <summary>
		/// Наименования оснований скидки через запятую
		/// </summary>
		public virtual string DiscountReasonsNames =>
			string.Join(", ", DiscountReasons.Select(x => x.Name));
		
		#region IGoods implementation

		IEnumerable<DiscountReason> IGoods.DiscountReasons => DiscountReasons;

		#endregion

		[Obsolete("В сигнатуре передается одно основание скидки, нужно использовать другой метод Create()")]
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
			return Create(
				nomenclatureId,
				count,
				isDiscountInMoney,
				isFixedPrice,
				discount,
				price,
				promoSetId,
				new List<DiscountReason> { discountReason },
				nomenclature,
				promotionalSet,
				onlineOrder);
		}
		
		public static OnlineOrderItem Create(
			int? nomenclatureId,
			decimal count,
			bool isDiscountInMoney,
			bool isFixedPrice,
			decimal discount,
			decimal price,
			int? promoSetId,
			IEnumerable<DiscountReason> discountReasons,
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
				Nomenclature = nomenclature,
				PromoSet = promotionalSet,
				OnlineOrder = onlineOrder
			};

			if(discountReasons != null)
			{
				foreach(var reason in discountReasons)
				{
					if(reason != null)
					{
						onlineOrderItem.DiscountReasons.Add(reason);
					}
				}
			}

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

			var currentPrice = CurrentRawPrice;
			var totalDiscountMoney = CalculateTotalDiscountInMoneyFromAddedReasons();

			DiscountMoneyFromDiscountReasons =
				DiscountReasons.All(x => x.ValueType == DiscountUnits.money)
				? DiscountReasons.Sum(x => x.Value)
				: totalDiscountMoney;

			DiscountPercentFromDiscountReasons =
				DiscountReasons.All(x => x.ValueType == DiscountUnits.percent)
				? DiscountReasons.Sum(x => x.Value)
				: currentPrice > 0 ? (100 * totalDiscountMoney) / currentPrice : 0;

			IsDiscountInMoneyFromDiscountReasons = DiscountReasons.Any(x => x.ValueType == DiscountUnits.money);
		}

		private decimal CurrentRawPrice => Price * CurrentCount;

		private decimal CalculateTotalDiscountInMoneyFromAddedReasons()
		{
			decimal currentPrice = CurrentRawPrice;

			decimal totalPercentDiscount = 0;
			decimal totalMoneyDiscount = 0;

			foreach(var reason in DiscountReasons)
			{
				if(reason.ValueType == DiscountUnits.money)
				{
					totalMoneyDiscount += reason.Value;
				}
				else
				{
					totalPercentDiscount += reason.Value;
				}
			}

			decimal discountFromPercent = currentPrice * (totalPercentDiscount / 100);
			decimal totalDiscountMoney = discountFromPercent + totalMoneyDiscount;

			return totalDiscountMoney;
		}
	}
}
