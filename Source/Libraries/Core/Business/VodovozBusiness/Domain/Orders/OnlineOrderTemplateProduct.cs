using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Товары автозаказов с ИПЗ",
		Nominative = "Товар автозаказа с ИПЗ",
		Prepositional = "Товарe автозаказа с ИПЗ",
		PrepositionalPlural = "Товарах автозаказов с ИПЗ"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplateProduct : PropertyChangedBase, IDomainObject, ICalculatingPrice
	{
		private int? _nomenclatureId;
		private decimal _price;
		private bool _isDiscountInMoney;
		private decimal _discount;
		private int? _promoSetId;
		private int _onlineOrderTemplateId;
		private decimal _count;
		private DiscountReason _discountReason;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;

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
		/// Скидка в деньгах
		/// </summary>
		[Display(Name = "Скидка в деньгах")]
		public virtual bool IsDiscountInMoney
		{
			get => _isDiscountInMoney;
			set => SetField(ref _isDiscountInMoney, value);
		}
		
		/// <summary>
		/// Скидка
		/// </summary>
		[Display(Name = "Скидка")]
		public virtual decimal Discount
		{
			get => _discount;
			set => SetField(ref _discount, value);
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
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			protected set => SetField(ref _count, value);
		}

		/// <summary>
		/// Основание скидки на товар
		/// </summary>
		[Display(Name = "Основание скидки на товар")]
		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}

		public virtual bool IsFixedPrice { get; }
		
		public virtual decimal Sum => Price * Count;
		
		public static OnlineOrderTemplateProduct Create(
			int? nomenclatureId,
			decimal count,
			bool isDiscountInMoney,
			decimal discount,
			decimal price,
			int? promoSetId,
			DiscountReason discountReason,
			Nomenclature nomenclature,
			PromotionalSet promotionalSet,
			int templateId
		)
		{
			var onlineOrderItem = new OnlineOrderTemplateProduct
			{
				NomenclatureId = nomenclatureId,
				Count = count,
				IsDiscountInMoney = isDiscountInMoney,
				Discount =  discount,
				Price = price,
				PromoSetId = promoSetId,
				DiscountReason = discountReason,
				Nomenclature = nomenclature,
				PromoSet = promotionalSet,
				OnlineOrderTemplateId = templateId
			};

			return onlineOrderItem;
		}
	}
}
