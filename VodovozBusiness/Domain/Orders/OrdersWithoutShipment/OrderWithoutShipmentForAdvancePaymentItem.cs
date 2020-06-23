using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Parameters;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки заказа без отгрузки на предоплату",
		Nominative = "строка заказа без отгрузки на предоплату")]
	public class OrderWithoutShipmentForAdvancePaymentItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		OrderWithoutShipmentForAdvancePayment orderWithoutDeliveryForAdvancePayment;
		[Display(Name = "Заказ без отгрузки на предоплату")]
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
					if(Id == 0)//ставку устанавливаем только для новых строк заказа
						switch(value.VAT) {
							case VAT.No:
								ValueAddedTax = 0m;
								break;
							case VAT.Vat10:
								ValueAddedTax = 0.10m;
								break;
							case VAT.Vat18:
								ValueAddedTax = 0.18m;
								break;
							case VAT.Vat20:
								ValueAddedTax = 0.20m;
								break;
							default:
								ValueAddedTax = 0m;
								break;
						}
				}
			}
		}

		bool isUserPrice;
		[Display(Name = "Цена установлена пользователем")]
		public virtual bool IsUserPrice {
			get => isUserPrice;
			set => SetField(ref isUserPrice, value);
		}

		/*AdditionalAgreement additionalAgreement;
		[Display(Name = "Дополнительное соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get => additionalAgreement;
			set => SetField(ref additionalAgreement, value, () => AdditionalAgreement);
		}*/

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
					/*if(AdditionalAgreement?.Self is SalesEquipmentAgreement aa)
						aa.UpdatePrice(Nomenclature, value);*/
					RecalculateDiscount();
					RecalculateNDS();
				}
			}
		}

		int count = -1;
		[Display(Name = "Количество")]
		public virtual int Count {
			get => count;
			set {
				if(SetField(ref count, value)) {
					/*if(AdditionalAgreement?.Self is SalesEquipmentAgreement aa)
						aa.UpdateCount(Nomenclature, value);*/
					OrderWithoutDeliveryForAdvancePayment?.RecalculateItemsPrice();
					RecalculateDiscount();
					RecalculateNDS();
				}
			}
		}

		int? actualCount;
		public virtual int? ActualCount {
			get => actualCount;
			set {
				if(SetField(ref actualCount, value)) {
					RecalculateDiscount();
					RecalculateNDS();
				}
			}
		}

		decimal includeNDS;
		[Display(Name = "Включая НДС")]
		public virtual decimal IncludeNDS {
			get => includeNDS;
			set => SetField(ref includeNDS, value);
		}

		private bool isDiscountInMoney;
		[Display(Name = "Скидка деньгами?")]
		public virtual bool IsDiscountInMoney {
			get => isDiscountInMoney;
			set {
				if(SetField(ref isDiscountInMoney, value))
					RecalculateNDS();
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
					RecalculateNDS();
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
				//value = value > Price * CurrentCount ? Price * CurrentCount : value;
				if(value != discountMoney && value == 0) {
					DiscountReason = null;
				}
				if(SetField(ref discountMoney, value))
					RecalculateNDS();
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

		/*PromotionalSet promoSet;
		[Display(Name = "Добавлено из промо-набора")]
		public virtual PromotionalSet PromoSet {
			get => promoSet;
			set => SetField(ref promoSet, value);
		}*/

		FreeRentEquipment freeRentEquipment;

		public virtual FreeRentEquipment FreeRentEquipment {
			get => freeRentEquipment;
			set => SetField(ref freeRentEquipment, value);
		}

		PaidRentEquipment paidRentEquipment;

		public virtual PaidRentEquipment PaidRentEquipment {
			get => paidRentEquipment;
			set => SetField(ref paidRentEquipment, value);
		}

		int RentEquipmentCount {
			get {
				/*if(AdditionalAgreement?.Type == AgreementType.NonfreeRent && PaidRentEquipment != null)
					return PaidRentEquipment.Count;

				if(AdditionalAgreement?.Type == AgreementType.FreeRent && FreeRentEquipment != null)
					return FreeRentEquipment.Count;*/

				return 0;
			}
		}

		int RentTime {
			get {
				/*if(AdditionalAgreement == null) {
					return 0;
				}
				if(AdditionalAgreement.Self is NonfreeRentAgreement) {
					NonfreeRentAgreement nonFreeRent = AdditionalAgreement.Self as NonfreeRentAgreement;
					if(nonFreeRent.RentMonths.HasValue) {
						return nonFreeRent.RentMonths.Value;
					}
				}

				if(AdditionalAgreement.Self is DailyRentAgreement) {
					DailyRentAgreement dailyRent = AdditionalAgreement.Self as DailyRentAgreement;
					return dailyRent.RentDays;
				}*/

				return 0;
			}
		}

		public int CurrentCount => ActualCount ?? Count;

		public virtual decimal Sum => Price * Count - DiscountMoney;//FIXME Count -- CurrentCount

		public virtual decimal ActualSum => Price * CurrentCount - DiscountMoney;

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
				/*if(PromoSet != null) {
					return false;
				}
				if(IsRentRenewal())
					return true;
					*/
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

			if(CurrentCount == 0)
				RemoveDiscount();
			else
				CalculateAndSetDiscount(DiscountSetter);
		}

		void RemoveDiscount()
		{
			/*if(DiscountMoney > 0) {
				OriginalDiscountMoney = DiscountMoney;
				OriginalDiscountReason = DiscountReason;
				OriginalDiscount = Discount;
			}*/
			DiscountReason = null;
			DiscountMoney = 0;
			Discount = 0;
		}

		public virtual void RecalculatePrice()
		{
			if(IsUserPrice/* || PromoSet != null*/)
				return;

			Price = GetPriceByTotalCount();
		}

		private void CalculateAndSetDiscount(decimal value)
		{
			if((Price * CurrentCount) == 0) {
				DiscountMoney = 0;
				Discount = 0;
				return;
			}
			if(IsDiscountInMoney) {
				DiscountMoney = value > Price * CurrentCount ? Price * CurrentCount : (value < 0 ? 0 : value);
				Discount = (100 * DiscountMoney) / (Price * CurrentCount);
			} else {
				Discount = value > 100 ? 100 : (value < 0 ? 0 : value);
				DiscountMoney = Price * CurrentCount * Discount / 100;
			}
		}

		public virtual decimal GetPriceByTotalCount()
		{
			if(Nomenclature != null) {
				if(Nomenclature.DependsOnNomenclature == null)
					return Nomenclature.GetPrice(Nomenclature.IsWater19L ? OrderWithoutDeliveryForAdvancePayment?.GetTotalWater19LCount(doNotCountWaterFromPromoSets: true) : Count);
				if(Nomenclature.IsWater19L)
					return Nomenclature.DependsOnNomenclature.GetPrice(Nomenclature.IsWater19L ? OrderWithoutDeliveryForAdvancePayment?.GetTotalWater19LCount(doNotCountWaterFromPromoSets: true) : Count);
			}
			return 0m;
		}

		void RecalculateNDS()
		{
			if(ValueAddedTax.HasValue)
				IncludeNDS = Math.Round(ActualSum * ValueAddedTax.Value / (1 + ValueAddedTax.Value), 2);
		}

		public OrderWithoutShipmentForAdvancePaymentItem() { }
	}
}
