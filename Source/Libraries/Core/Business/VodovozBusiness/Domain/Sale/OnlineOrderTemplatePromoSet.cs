using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	public class OnlineOrderTemplatePromoSet : PropertyChangedBase, IDomainObject, ICalculatingTotalMoneyDiscount
	{
		private decimal _count;
		private decimal _price;
		private int _templateId;
		private PromotionalSet _promoSet;
		private IObservableList<DiscountReasonBase> _discountReasons = new ObservableList<DiscountReasonBase>();

		protected OnlineOrderTemplatePromoSet() { }

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
		/// Онлайн заказ
		/// </summary>
		[Display(Name = "Онлайн заказ")]
		public virtual int TemplateId
		{
			get => _templateId;
			set => SetField(ref _templateId, value);
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
		public virtual IObservableList<DiscountReasonBase> DiscountReasons
		{
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}
		
		/// <summary>
		/// Название промонабора
		/// </summary>
		public virtual string PromoSetName => PromoSet?.Name;

		/// <inheritdoc/>
		public virtual decimal CurrentRawPrice => PromoSet?.SumWithoutDiscount() ?? 0m;

		/// <inheritdoc/>
		IEnumerable<DiscountReasonBase> IDiscountReasons.DiscountReasons => DiscountReasons;

		/// <inheritdoc/>
		public virtual PersonalDiscount PersonalDiscount => null;

		public static OnlineOrderTemplatePromoSet Create(
			int templateId,
			decimal count,
			decimal price,
			PromotionalSet promoSet,
			IList<DiscountReasonBase> discountReasons = null
		)
		{
			var onlinePromoSet = new OnlineOrderTemplatePromoSet
			{
				Count = count,
				Price = price,
				TemplateId = templateId,
				PromoSet = promoSet
			};

			if(discountReasons == null || !discountReasons.Any())
			{
				return onlinePromoSet;
			}

			foreach(var discountReason in discountReasons)
			{
				onlinePromoSet.DiscountReasons.Add(discountReason);
			}

			return onlinePromoSet;
		}
	}
}
