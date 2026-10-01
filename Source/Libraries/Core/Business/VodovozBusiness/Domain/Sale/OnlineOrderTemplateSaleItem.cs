using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Продаваемые позиции автозаказов с ИПЗ",
		Nominative = "Продаваемая позиция автозаказа с ИПЗ",
		Prepositional = "Продаваемой позиции автозаказа с ИПЗ",
		PrepositionalPlural = "Продаваемых позициях автозаказов с ИПЗ"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplateSaleItem : PropertyChangedBase, IDomainObject, ISaleItem
	{
		private decimal _price;
		private int _templateId;
		private decimal _count;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;
		private PersonalDiscount _personalDiscount;
		private IObservableList<DiscountReasonBase> _discountReasons = new ObservableList<DiscountReasonBase>();

		protected OnlineOrderTemplateSaleItem()
		{
		}

		public virtual int Id { get; set; }

		/// <summary>
		/// Шаблон автозаказа
		/// </summary>
		[Display(Name = "Идентификатор шаблона автозаказа")]
		public virtual int TemplateId
		{
			get => _templateId;
			set => SetField(ref _templateId, value);
		}

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
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			protected set => SetField(ref _nomenclature, value);
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
		/// Список оснований скидок
		/// </summary>
		[Display(Name = "Список оснований скидок")]
		public virtual IObservableList<DiscountReasonBase> DiscountReasons
		{
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}
		
		/// <summary>
		/// Персональная скидка
		/// </summary>
		[Display(Name = "Персональная скидка")]
		public virtual PersonalDiscount PersonalDiscount
		{
			get => _personalDiscount;
			set => SetField(ref _personalDiscount, value);
		}

		/*IEnumerable<IProductDiscountData> ICalculatingPriceV5.Discounts => Discounts
			.Select(x => new ProductDiscountData
			{
				Discount = x.IsDiscountInMoney ? x.MoneyDiscount : x.PercentDiscount,
				IsDiscountInMoney = x.IsDiscountInMoney,
				DiscountReason = x.DiscountReason
			});*/

		public virtual bool IsUserPrice { get; set; }
		
		public virtual bool IsFixedPrice { get; set; }

		public virtual decimal Sum => Math.Round(Price * Count, 2);

		public static OnlineOrderTemplateSaleItem Create(
			int templateId,
			NewOrderSaleItem newSaleItem,
			PersonalDiscount personalDiscount = null
		)
		{
			var onlineOrderItem = new OnlineOrderTemplateSaleItem
			{
				Count = newSaleItem.Count,
				Price = newSaleItem.PriceData.Price,
				Nomenclature = newSaleItem.Nomenclature,
				PromoSet = newSaleItem.PromoSet,
				TemplateId = templateId,
				PersonalDiscount = personalDiscount
			};

			if(newSaleItem.DiscountReasons != null && newSaleItem.DiscountReasons.Any())
			{
				foreach(var discount in newSaleItem.DiscountReasons)
				{
					onlineOrderItem.DiscountReasons.Add(discount);
				}
			}

			return onlineOrderItem;
		}

		public virtual bool IsAlternativePrice { get; set; }
		public virtual bool IsMasterNomenclature => Nomenclature != null && Nomenclature.Category == NomenclatureCategory.master;

		decimal ISetCount.Count
		{
			get => Count;
			set => Count = value;
		}

		IEnumerable<DiscountReasonBase> IDiscountReasons.DiscountReasons => DiscountReasons;
	}
}
