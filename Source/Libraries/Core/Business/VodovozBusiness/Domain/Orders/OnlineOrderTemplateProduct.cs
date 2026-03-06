using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders.V5;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Товары автозаказов с ИПЗ",
		Nominative = "Товар автозаказа с ИПЗ",
		Prepositional = "Товарe автозаказа с ИПЗ",
		PrepositionalPlural = "Товарах автозаказов с ИПЗ"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplateProduct : PropertyChangedBase, IDomainObject, ICalculatingPriceV5
	{
		private int? _nomenclatureId;
		private decimal _price;
		private int? _promoSetId;
		private int _onlineOrderTemplateId;
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
		public virtual int OnlineOrderTemplateId
		{
			get => _onlineOrderTemplateId;
			set => SetField(ref _onlineOrderTemplateId, value);
		}
		
		/// <summary>
		/// Id номенклатуры
		/// </summary>
		[Display(Name = "Id номенклатуры")]
		public virtual int? NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
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
		/// Id промонабора
		/// </summary>
		[Display(Name = "Id промонабора")]
		public virtual int? PromoSetId
		{
			get => _promoSetId;
			set => SetField(ref _promoSetId, value);
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
		public virtual IObservableList<OnlineOrderTemplateProductDiscount> Discounts
		{
			get => _discounts;
			set => SetField(ref _discounts, value);
		}
		
		IEnumerable<IDiscountData> ICalculatingPriceV5.Discounts => Discounts
			.Select(x => new ProductDiscountData
			{
				Discount = x.IsDiscountInMoney ? x.MoneyDiscount : x.PercentDiscount,
				IsDiscountInMoney = x.IsDiscountInMoney,
				DiscountReason = x.DiscountReason
			});

		public virtual bool IsFixedPrice { get; }
		
		public virtual decimal Sum => Price * Count;
		
		public static OnlineOrderTemplateProduct Create(
			int? nomenclatureId,
			decimal count,
			decimal price,
			int? promoSetId,
			Nomenclature nomenclature,
			PromotionalSet promotionalSet,
			int templateId,
			IObservableList<OnlineOrderTemplateProductDiscount> discounts
		)
		{
			var onlineOrderItem = new OnlineOrderTemplateProduct
			{
				NomenclatureId = nomenclatureId,
				Count = count,
				Price = price,
				PromoSetId = promoSetId,
				Nomenclature = nomenclature,
				PromoSet = promotionalSet,
				OnlineOrderTemplateId = templateId,
				Discounts = discounts
			};

			return onlineOrderItem;
		}
	}
}
