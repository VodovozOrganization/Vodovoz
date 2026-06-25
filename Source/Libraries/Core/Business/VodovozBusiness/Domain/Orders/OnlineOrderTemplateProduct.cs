using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
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
	public class OnlineOrderTemplateProduct : PropertyChangedBase, IDomainObject, ICalculatingPriceWithManyDiscounts, IProduct
	{
		private decimal _price;
		private int _templateId;
		private int _rentCount;
		private decimal _count;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;
		private IObservableList<OnlineOrderTemplateProductDiscount> _discounts = new ObservableList<OnlineOrderTemplateProductDiscount>();
		private SaleRentType _rentType;
		private OrderItemRentSubType _orderItemRentSubType;
		private PaidRentPackage _paidRentPackage;
		private FreeRentPackage _freeRentPackage;

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
		/// Количество аренды (дни/месяцы)
		/// </summary>
		[Display(Name = "Количество аренды (дни/месяцы)")]
		public virtual int RentCount
		{
			get => _rentCount;
			protected set => SetField(ref _rentCount, value);
		}
		
		/// <summary>
		/// Тип аренды
		/// </summary>
		[Display(Name = "Тип аренды")]
		public virtual SaleRentType RentType
		{
			get => _rentType;
			set => SetField(ref _rentType, value);
		}

		/// <summary>
		/// Подтип позиции аренды
		/// </summary>
		[Display(Name = "Подтип позиции аренды")]
		public virtual OrderItemRentSubType OrderItemRentSubType
		{
			get => _orderItemRentSubType;
			set => SetField(ref _orderItemRentSubType, value);
		}

		/// <summary>
		/// Пакет платной аренды
		/// </summary>
		[Display(Name = "Пакет платной аренды")]
		public PaidRentPackage PaidRentPackage
		{
			get => _paidRentPackage;
			set => SetField(ref _paidRentPackage, value);
		}
		
		/// <summary>
		/// Пакет бесплатной аренды
		/// </summary>
		[Display(Name = "Пакет бесплатной аренды")]
		public FreeRentPackage FreeRentPackage
		{
			get => _freeRentPackage;
			set => SetField(ref _freeRentPackage, value);
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

		public virtual decimal GetDiscount => 0;

		public virtual bool IsDiscountInMoney => false;

		public virtual decimal ActualSum => Sum;

		public virtual decimal CurrentCount => Count;

		public virtual DiscountReason DiscountReason => null;
		
		public virtual bool IsCopiedFromUndelivery => false;
		
		public virtual bool IsAlternativePrice { get; set; }

		public virtual bool IsMasterNomenclature => Nomenclature != null && Nomenclature.Category == NomenclatureCategory.master;

		public virtual void RecalculatePrice()
		{
			throw new NotImplementedException();
		}
		
		public virtual bool IsUserPrice { get; }
		public virtual void SetPrice(decimal price)
		{
			throw new NotImplementedException();
		}

		internal static OnlineOrderTemplateProduct Create(
			int templateId,
			decimal count,
			decimal price,
			Nomenclature nomenclature,
			PromotionalSet promotionalSet,
			IObservableList<OnlineOrderTemplateProductDiscount> discounts
		)
		{
			var onlineOrderItem = new OnlineOrderTemplateProduct
			{
				TemplateId = templateId,
				Count = count,
				Price = price,
				Nomenclature = nomenclature,
				PromoSet = promotionalSet,
				Discounts = discounts
			};

			return onlineOrderItem;
		}
		
		internal static OnlineOrderTemplateProduct CreateNewNonFreeRentServiceItem(
			int templateId,
			PaidRentPackage paidRentPackage
			)
		{
			var newItem = new OnlineOrderTemplateProduct
			{
				TemplateId = templateId,
				Count = 1,
				RentCount = 1,
				RentType = SaleRentType.NonFreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentServiceItem,
				PaidRentPackage = paidRentPackage,
				Nomenclature = paidRentPackage.RentServiceMonthly
			};

			newItem.UpdatePriceWithRecalculate(paidRentPackage.PriceMonthly);

			return newItem;
		}

		internal static OnlineOrderTemplateProduct CreateNewNonFreeRentDepositItem(
			int templateId,
			PaidRentPackage paidRentPackage
			)
		{
			var newItem = new OnlineOrderTemplateProduct
			{
				TemplateId = templateId,
				Count = 1,
				RentType = SaleRentType.NonFreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				PaidRentPackage = paidRentPackage,
				Nomenclature = paidRentPackage.DepositService
			};

			newItem.UpdatePriceWithRecalculate(paidRentPackage.Deposit);

			return newItem;
		}
		
		internal static OnlineOrderTemplateProduct CreateNewDailyRentServiceItem(
			int templateId,
			PaidRentPackage paidRentPackage
			)
		{
			var newItem = new OnlineOrderTemplateProduct
			{
				TemplateId = templateId,
				Count = 1,
				RentCount = 1,
				RentType = SaleRentType.DailyRent,
				OrderItemRentSubType = OrderItemRentSubType.RentServiceItem,
				PaidRentPackage = paidRentPackage,
				Nomenclature = paidRentPackage.RentServiceDaily
			};

			newItem.UpdatePriceWithRecalculate(paidRentPackage.PriceDaily);
			return newItem;
		}

		internal static OnlineOrderTemplateProduct CreateNewDailyRentDepositItem(
			int templateId,
			PaidRentPackage paidRentPackage
			)
		{
			var newItem = new OnlineOrderTemplateProduct
			{
				TemplateId = templateId,
				Count = 1,
				RentType = SaleRentType.DailyRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				PaidRentPackage = paidRentPackage,
				Nomenclature = paidRentPackage.DepositService
			};

			newItem.UpdatePriceWithRecalculate(paidRentPackage.Deposit);
			return newItem;
		}

		internal static OnlineOrderTemplateProduct CreateNewFreeRentDepositItem(
			int templateId,
			FreeRentPackage freeRentPackage)
		{
			var newItem = new OnlineOrderTemplateProduct
			{
				TemplateId = templateId,
				Count = 1,
				RentType = SaleRentType.FreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				FreeRentPackage = freeRentPackage,
				Nomenclature = freeRentPackage.DepositService
			};

			newItem.UpdatePriceWithRecalculate(freeRentPackage.Deposit);
			return newItem;
		}
	}
}
