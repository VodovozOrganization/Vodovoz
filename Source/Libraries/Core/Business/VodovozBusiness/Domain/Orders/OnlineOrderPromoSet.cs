using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Промонаборы онлайн заказа",
		Nominative = "Промонабор онлайн заказа",
		Prepositional = "Промонаборе онлайн заказа",
		PrepositionalPlural = "Промонаборах онлайн заказа"
	)]
	[HistoryTrace]
	public class OnlineOrderPromoSet : PropertyChangedBase, IDomainObject
	{
		private int _receivedPromoSetId;
		private decimal _count = -1;
		private decimal _price;
		private OnlineOrder _onlineOrder;
		private PromotionalSet _promoSet;
		private IList<DiscountReason> _discountReasons = new List<DiscountReason>();

		protected OnlineOrderPromoSet() { }

		public virtual int Id { get; set; }
		
		/// <summary>
		/// Цена
		/// </summary>
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			protected set => SetField(ref _count, value);
		}

		/// <summary>
		/// Идентификатор промонабора
		/// </summary>
		[Display(Name = "Id промонабора")]
		public virtual int ReceivedPromoSetId
		{
			get => _receivedPromoSetId;
			set => SetField(ref _receivedPromoSetId, value);
		}

		/// <summary>
		/// Онлайн заказ
		/// </summary>
		[Display(Name = "Онлайн заказ")]
		public virtual OnlineOrder OnlineOrder
		{
			get => _onlineOrder;
			set => SetField(ref _onlineOrder, value);
		}

		/// <summary>
		/// Промонабор
		/// </summary>
		[Display(Name = "Промонабор")]
		public virtual PromotionalSet PromoSet
		{
			get => _promoSet;
			set => SetField(ref _promoSet, value);
		}
		
		/// <summary>
		/// Основания скидок на товар
		/// </summary>
		[Display(Name = "Основания скидок на товар")]
		public virtual IList<DiscountReason> DiscountReasons
		{
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}

		public static OnlineOrderPromoSet Create(
			int promoSetId,
			decimal count,
			decimal price,
			OnlineOrder onlineOrder,
			PromotionalSet promoSet,
			IList<DiscountReason> discountReasons = null
		) => new OnlineOrderPromoSet
		{
			ReceivedPromoSetId = promoSetId,
			Count = count,
			Price = price,
			OnlineOrder = onlineOrder,
			PromoSet = promoSet,
			DiscountReasons = discountReasons
		};
	}
}
