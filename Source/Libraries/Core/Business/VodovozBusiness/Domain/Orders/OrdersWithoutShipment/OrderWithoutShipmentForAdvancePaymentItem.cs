using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки счета без отгрузки на предоплату",
		Nominative = "строка счета без отгрузки на предоплату")]
	public class OrderWithoutShipmentForAdvancePaymentItem : PropertyChangedBase, IDomainObject, IDiscount
	{
		private bool _isAlternativePrice;

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
			set {
				if(SetField(ref nomenclature, value)) {
					CalculateVATType();
				}
			}
		}

		bool isUserPrice;
		[Display(Name = "Цена установлена пользователем")]
		public virtual bool IsUserPrice {
			get => isUserPrice;
			set => SetField(ref isUserPrice, value);
		}
		
		decimal price;
		[Display(Name = "Цена")]
		public virtual decimal Price {
			get => price;
			set {
				//Если цена не отличается от той которая должна быть по прайсам в 
				//номенклатуре, то цена не изменена пользователем и сможет расчитываться автоматически
				IsUserPrice = value != GetPriceByTotalCount() && value != 0;
				if(IsUserPrice)
					IsUserPrice = value != GetPriceByTotalCount() && value != 0;

				if(SetField(ref price, value)) {
					RecalculateDiscount();
					RecalculateVAT();
				}
			}
		}

		decimal count = -1;
		[Display(Name = "Количество")]
		public virtual decimal Count {
			get => count;
			set {
				if(SetField(ref count, value)) {
					OrderWithoutDeliveryForAdvancePayment?.RecalculateItemsPrice();
					RecalculateDiscount();
					RecalculateVAT();
				}
			}
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
			set {
				if(SetField(ref isDiscountInMoney, value))
					RecalculateVAT();
			}
		}

		private decimal discount;
		[Display(Name = "Процент скидки на товар")]
		public virtual decimal Discount {
			get => discount;
			set {
				if(value != discount && value == 0) {
					DiscountReason = null;
				}
				if(SetField(ref discount, value)) {
					RecalculateVAT();
				}
			}
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
			set {
				if(value != discountMoney && value == 0) {
					DiscountReason = null;
				}
				if(SetField(ref discountMoney, value))
					RecalculateVAT();
			}
		}

		private decimal discountByStock;
		[Display(Name = "Скидка по акции")]
		public virtual decimal DiscountByStock {
			get => discountByStock;
			set => SetField(ref discountByStock, value);
		}

		private DiscountReason discountReason;
		[Display(Name = "Основание скидки на товар")]
		public virtual DiscountReason DiscountReason {
			get => discountReason;
			set => SetField(ref discountReason, value);
		}

		[Display(Name = "Альтернативная цена?")]
		public virtual bool IsAlternativePrice
		{
			get => _isAlternativePrice;
			set => SetField(ref _isAlternativePrice, value);
		}

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

		public virtual decimal ManualChangingDiscount {
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set {
				CalculateAndSetDiscount(value);
				if(DiscountByStock != 0) {
					DiscountByStock = 0;
					DiscountReason = null;
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

		public virtual decimal DiscountSetter {
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set => CalculateAndSetDiscount(value);
		}

		private void RecalculateDiscount()
		{
			if(!NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(DiscountMoney))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Discount))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Price))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Count))
			   || (OrderWithoutDeliveryForAdvancePayment == null || !NHibernate.NHibernateUtil.IsInitialized(OrderWithoutDeliveryForAdvancePayment.OrderWithoutDeliveryForAdvancePaymentItems))) {
				return;
			}

			if(Count == 0)
				RemoveDiscount();
			else
				CalculateAndSetDiscount(DiscountSetter);
		}

		void RemoveDiscount()
		{
			DiscountReason = null;
			DiscountMoney = 0;
			Discount = 0;
		}

		public virtual void RecalculatePrice()
		{
			if(IsUserPrice)
				return;

			Price = GetPriceByTotalCount();
		}

		private void CalculateAndSetDiscount(decimal value)
		{
			if((Price * Count) == 0) {
				DiscountMoney = 0;
				Discount = 0;
				return;
			}
			if(IsDiscountInMoney) {
				DiscountMoney = value > Price * Count ? Price * Count : (value < 0 ? 0 : value);
				Discount = (100 * DiscountMoney) / (Price * Count);
			} else {
				Discount = value > 100 ? 100 : (value < 0 ? 0 : value);
				DiscountMoney = Price * Count * Discount / 100;
			}
		}

		public virtual decimal GetPriceByTotalCount()
		{
			if(Nomenclature != null)
			{
				var curCount = Nomenclature.IsWater19L ? OrderWithoutDeliveryForAdvancePayment.GetTotalWater19LCount() : Count;
				var canApplyAlternativePrice = OrderWithoutDeliveryForAdvancePayment.HasPermissionsForAlternativePrice
				                               && Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount);

				if(Nomenclature.DependsOnNomenclature == null)
					return Nomenclature.GetPrice(curCount, canApplyAlternativePrice);
				if(Nomenclature.IsWater19L)
					return Nomenclature.DependsOnNomenclature.GetPrice(curCount, canApplyAlternativePrice);
			}
			return 0m;
		}

		public virtual void CalculateVATType()
		{
			if(!NHibernateUtil.IsInitialized(Nomenclature)) {
				NHibernateUtil.Initialize(Nomenclature);
			}
			if(OrderWithoutDeliveryForAdvancePayment == null) {
				return;
			}
			
			if(!NHibernateUtil.IsInitialized(OrderWithoutDeliveryForAdvancePayment)) {
				NHibernateUtil.Initialize(OrderWithoutDeliveryForAdvancePayment);
			}
			
			var vatRateVersion = Nomenclature.GetActualVatRateVersion(OrderWithoutDeliveryForAdvancePayment.DocumentDate);
			if(vatRateVersion == null)
			{
				throw new InvalidOperationException($"У товара #{Nomenclature.Id} отсутствует версия НДС на дату доставки счета #{OrderWithoutDeliveryForAdvancePayment.DocumentDate}");
			}
			
			ValueAddedTax = CanUseVAT() ? vatRateVersion.VatRate.VatNumericValue : 0;
		}
		
		private void RecalculateVAT()
		{
			if(OrderWithoutDeliveryForAdvancePayment == null) {
				return;
			}

			if(!NHibernateUtil.IsInitialized(OrderWithoutDeliveryForAdvancePayment)) {
				NHibernateUtil.Initialize(OrderWithoutDeliveryForAdvancePayment);
			}
			
			if(!CanUseVAT()) {
				IncludeNDS = null;
				return;
			}

			if(CanUseVAT() && ValueAddedTax.HasValue) {
				IncludeNDS = Math.Round(Sum * ValueAddedTax.Value / (1 + ValueAddedTax.Value), 2);
			}
		}

		private bool CanUseVAT()
		{
			bool canUseVAT = true;
			var organization = OrderWithoutDeliveryForAdvancePayment.Organization;
			if(organization != null) {
				canUseVAT = !organization.WithoutVAT;
			}

			return canUseVAT;
		}
		
		public void SetDiscount(bool isDiscountInMoney, decimal discount, DiscountReason discountReason)
		{
			IsDiscountInMoney = isDiscountInMoney;
			Discount = discount;
			DiscountReason = discountReason;
		}

		public OrderWithoutShipmentForAdvancePaymentItem() { }
	}
}
