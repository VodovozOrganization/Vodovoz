using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NPOI.SS.Formula.Functions;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Nodes;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Товары автозаказов с ИПЗ",
		Nominative = "Товар автозаказа с ИПЗ",
		Prepositional = "Товарe автозаказа с ИПЗ",
		PrepositionalPlural = "Товарах автозаказов с ИПЗ"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplateProduct : PropertyChangedBase, IDomainObject, ICalculatingPriceWithManyDiscounts
	{
		private decimal _price;
		private int _templateId;
		private decimal _count;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;
		private IObservableList<OnlineOrderTemplateProductDiscount> _discounts = new ObservableList<OnlineOrderTemplateProductDiscount>();

		protected OnlineOrderTemplateProduct() { }

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
		/// Список скидок
		/// </summary>
		[Display(Name = "Список скидок")]
		public virtual IObservableList<OnlineOrderTemplateProductDiscount> Discounts
		{
			get => _discounts;
			set => SetField(ref _discounts, value);
		}
		
		IEnumerable<IDiscountData> ICalculatingPriceWithManyDiscounts.Discounts => Discounts
			.Select(x => DiscountData.Create(
				x.IsDiscountInMoney,
				x.IsDiscountInMoney ? x.MoneyDiscount : x.PercentDiscount,
				x.DiscountReason));

		public virtual bool IsFixedPrice { get; }
		
		public virtual decimal Sum => Math.Round(Price * Count, 2);
		
		public static OnlineOrderTemplateProduct Create(
			decimal count,
			decimal price,
			Nomenclature nomenclature,
			PromotionalSet promotionalSet,
			int templateId,
			IObservableList<OnlineOrderTemplateProductDiscount> discounts
		)
		{
			var onlineOrderItem = new OnlineOrderTemplateProduct
			{
				Count = count,
				Price = price,
				Nomenclature = nomenclature,
				PromoSet = promotionalSet,
				TemplateId = templateId,
				Discounts = discounts
			};

			return onlineOrderItem;
		}
	}
}
