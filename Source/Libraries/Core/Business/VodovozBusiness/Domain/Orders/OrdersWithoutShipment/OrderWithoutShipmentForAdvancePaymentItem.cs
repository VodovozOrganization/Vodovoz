using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки счета без отгрузки на предоплату",
		Nominative = "строка счета без отгрузки на предоплату")]
	public class OrderWithoutShipmentForAdvancePaymentItem
		: PropertyChangedBase,
			IDomainObject,
			IApplyDiscountReasonItem,
			IRecalculateTax,
			ISaleItem
	{
		private PersonalDiscount _personalDiscount;
		private IObservableList<DiscountReason> _discountReasons = new ObservableList<DiscountReason>();
		private bool _isAlternativePrice;
		private bool _isFixedPrice;

		public virtual int Id { get; set; }

		OrderWithoutShipmentForAdvancePayment orderWithoutDeliveryForAdvancePayment;
		[Display(Name = "Счет без отгрузки на предоплату")]
		public virtual OrderWithoutShipmentForAdvancePayment OrderWithoutDeliveryForAdvancePayment {
			get => orderWithoutDeliveryForAdvancePayment;
			set => SetField(ref orderWithoutDeliveryForAdvancePayment, value);
		}

		Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			protected set => SetField(ref nomenclature, value);
		}

		bool isUserPrice;
		[Display(Name = "Цена установлена пользователем")]
		public virtual bool IsUserPrice {
			get => isUserPrice;
			set => SetField(ref isUserPrice, value);
		}
		
		decimal price;
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => price;
			protected set => SetField(ref price, value);
		}

		decimal count = -1;
		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => count;
			protected set => SetField(ref count, value);
		}

		decimal? includeNDS;
		[Display(Name = "Включая НДС")]
		public virtual decimal? IncludeNDS {
			get => includeNDS;
			set => SetField(ref includeNDS, value);
		}

		private bool isDiscountInMoney;
		[Display(Name = "Скидка деньгами?")]
		public virtual bool IsDiscountInMoney {
			get => isDiscountInMoney;
			protected set => SetField(ref isDiscountInMoney, value);
		}

		private decimal discount;
		[Display(Name = "Процент скидки на товар")]
		public virtual decimal Discount {
			get => discount;
			protected set => SetField(ref discount, value);
		}

		decimal? valueAddedTax;
		[Display(Name = "НДС на момент создания заказа")]
		public virtual decimal? ValueAddedTax {
			get => valueAddedTax;
			set => SetField(ref valueAddedTax, value);
		}

		private decimal discountMoney;
		[Display(Name = "Скидка на товар в деньгах")]
		public virtual decimal DiscountMoney {
			get => discountMoney;
			protected set => SetField(ref discountMoney, value);
		}

		private decimal discountByStock;

		[Display(Name = "Скидка по акции")]
		public virtual decimal DiscountByStock {
			get => discountByStock;
			set => SetField(ref discountByStock, value);
		}

		[Display(Name = "Основания скидок на товар")]
		public virtual IObservableList<DiscountReason> DiscountReasons {
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}

		[Display(Name = "Альтернативная цена?")]
		public virtual bool IsAlternativePrice
		{
			get => _isAlternativePrice;
			set => SetField(ref _isAlternativePrice, value);
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

		#region IApplyDiscountReasonItem implementation

		/// <inheritdoc/>
		public decimal ActualSum => Sum;
		/// <inheritdoc/>
		public virtual bool IsFixedPrice => _isFixedPrice;
		
		/// <inheritdoc/>
		public virtual PromotionalSet PromoSet => null;
		/// <inheritdoc/>
		IList<DiscountReason> IApplyDiscountReasonItem.DiscountReasons => DiscountReasons;
		/// <inheritdoc/>
		decimal IApplyDiscountReasonItem.CurrentRawPrice => CurrentRawPrice;
		/// <inheritdoc/>
		public IDiscountValue DiscountData => DiscountValue.Create(IsDiscountInMoney, Discount, DiscountMoney);
		/// <inheritdoc/>
		public void SetDiscount(IDiscountValue discountValue)
		{
			IsDiscountInMoney = discountValue.IsDiscountMoney;
			DiscountMoney = discountValue.DiscountMoney;
			Discount = discountValue.Discount;
		}

		#endregion

		#region IRecalculateTax implementation

		public IRecalculateTaxSource RecalculateTaxSource => OrderWithoutDeliveryForAdvancePayment;
		IDepositNomenclature IRecalculateTax.Nomenclature => Nomenclature;

		#endregion

		#region ICount implementation

		decimal ISetCount.Count
		{
			get => Count;
			set
			{
				if(Count == value)
				{
					return;
				}
        		
				Count = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region ISaleItem implementation

		bool ISaleItem.IsFixedPrice
		{
			get => _isFixedPrice;
			set => _isFixedPrice = value;
		}

		PromotionalSet IGetFixedPrice.PromoSet => null;
		IEnumerable<DiscountReason> IDiscountReasons.DiscountReasons => DiscountReasons;
		
		#endregion
		
		#region IPrice implementation
		
		decimal IPrice.Price
		{
			get => Price;
			set
			{
				if(Price == value)
				{
					return;
				}
        		
				Price = value;
				OnPropertyChanged();
			}
		}

		#endregion

		int RentEquipmentCount {
			get {
				return 0;
			}
		}

		int RentTime {
			get {
				return 0;
			}
		}

		public virtual decimal Sum => Price * Count - DiscountMoney;

		public virtual decimal ManualChangingDiscount
		{
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set
			{
				if(IsDiscountInMoney)
				{
					DiscountMoney = value;
				}
				else
				{
					Discount = value;
				}
			}
		}

		public virtual bool IsRentCategory => RentEquipmentCount > 0;

		public virtual string RentString {
			get {
				int rentCount = RentTime;
				int count = RentEquipmentCount;

				if(rentCount != 0)
					return string.Format($"{count}*{rentCount}");
				return string.Empty;
			}
		}

		public virtual bool CanEditPrice {
			get {
				return Nomenclature.GetCategoriesWithEditablePrice().Contains(Nomenclature.Category);
			}
		}

		public virtual string NomenclatureString => Nomenclature != null ? Nomenclature.Name : string.Empty;

		/// <summary>
		/// Текущее количество товара
		/// </summary>
		public decimal CurrentCount => Count;

		private decimal CurrentRawPrice => Price * CurrentCount;

		/// <summary>
		/// Наименования оснований скидки через запятую
		/// </summary>
		public virtual string DiscountReasonsNames =>
			string.Join(", ", DiscountReasons.Select(x => x.Name));
		
		internal static OrderWithoutShipmentForAdvancePaymentItem Create(
			OrderWithoutShipmentForAdvancePayment orderWithoutShipment,
			decimal count,
			Nomenclature nomenclature,
			(SaleItemPriceType PriceType, decimal Price) priceData
		)
		{
			return new OrderWithoutShipmentForAdvancePaymentItem {
				OrderWithoutDeliveryForAdvancePayment = orderWithoutShipment,
				Count = count,
				Nomenclature = nomenclature,
				Price = priceData.Price
			};
		}
	}
}
