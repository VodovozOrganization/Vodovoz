using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QSSupportLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Repositories.Orders;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки заказа",
		Nominative = "строка заказа")]
	[HistoryTrace]
	public class OrderItem : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства

		public virtual int Id { get; set; }

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set {
				if(SetField(ref order, value, () => Order))
					RecalculateNDS();
			}
		}

		AdditionalAgreement additionalAgreement;

		[Display(Name = "Дополнительное соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get { return additionalAgreement; }
			set { SetField(ref additionalAgreement, value, () => AdditionalAgreement); }
		}

		Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				if(SetField(ref nomenclature, value, () => Nomenclature)) {
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

		Equipment equipment;

		[Display(Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField(ref equipment, value, () => Equipment); }
		}

		Decimal price;

		[Display(Name = "Цена")]
		public virtual Decimal Price {
			get { return price; }
			set {
				//Если цена не отличается от той которая должна быть по прайсам в 
				//номенклатуре, то цена не изменена пользователем и сможет расчитываться автоматически
				IsUserPrice = !(value == GetPriceByTotalCount() || value == 0);

				if(SetField(ref price, value, () => Price)) {
					if(AdditionalAgreement != null && AdditionalAgreement.Self is SalesEquipmentAgreement) {
						(AdditionalAgreement.Self as SalesEquipmentAgreement).UpdatePrice(Nomenclature, value);
					}
					RecalculateDiscount();
					RecalculateNDS();
				}
			}
		}

		bool isUserPrice;

		[Display(Name = "Цена установлена пользователем")]
		public virtual bool IsUserPrice {
			get { return isUserPrice; }
			set { SetField(ref isUserPrice, value, () => IsUserPrice); }
		}

		int count = -1;

		[Display(Name = "Количество")]
		public virtual int Count {
			get {
				return count;
			}
			set {
				if(SetField(ref count, value, () => Count)) {
					Order?.RecalculateItemsPrice();
					RecalculateDiscount();
					RecalculateNDS();
				}
			}
		}

		int actualCount;
		public virtual int ActualCount {
			get {
				return actualCount;
			}
			set {
				if(SetField(ref actualCount, value, () => ActualCount))
					RecalculateNDS();
			}
		}

		Decimal includeNDS;

		[Display(Name = "Включая НДС")]
		public virtual Decimal IncludeNDS {
			get { return includeNDS; }
			set { SetField(ref includeNDS, value, () => IncludeNDS); }
		}

		private bool isDiscountInMoney;

		[Display(Name = "Скидка деньгами?")]
		public virtual bool IsDiscountInMoney {
			get { return isDiscountInMoney; }
			set {
				if(SetField(ref isDiscountInMoney, value, () => IsDiscountInMoney))
					RecalculateNDS();
			}
		}

		private decimal discount;

		[Display(Name = "Процент скидки на товар")]
		public virtual decimal Discount {
			get { return discount; }
			set {
				if(value != discount && value == 0) {
					DiscountReason = null;
				}
				if(SetField(ref discount, value, () => Discount)) {
					RecalculateNDS();
				}
			}
		}

		private decimal discountMoney;

		[Display(Name = "Скидка на товар в деньгах")]
		public virtual decimal DiscountMoney {
			get { return discountMoney; }
			set {
				//value = value > Price * CurrentCount ? Price * CurrentCount : value;
				if(value != discountMoney && value == 0)
					DiscountReason = null;
				if(SetField(ref discountMoney, value, () => DiscountMoney))
					RecalculateNDS();
			}
		}

		decimal? valueAddedTax;
		[Display(Name = "НДС на момент создания заказа")]
		public virtual decimal? ValueAddedTax {
			get { return valueAddedTax; }
			set { SetField(ref valueAddedTax, value, () => ValueAddedTax); }
		}

		private DiscountReason discountReason;

		[Display(Name = "Основание скидки на товар")]
		public virtual DiscountReason DiscountReason {
			get { return discountReason; }
			set { SetField(ref discountReason, value, () => DiscountReason); }
		}

		CounterpartyMovementOperation counterpartyMovementOperation;

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation {
			get { return counterpartyMovementOperation; }
			set { SetField(ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation); }
		}

		FreeRentEquipment freeRentEquipment;

		public virtual FreeRentEquipment FreeRentEquipment {
			get { return freeRentEquipment; }
			set { SetField(ref freeRentEquipment, value, () => FreeRentEquipment); }
		}

		PaidRentEquipment paidRentEquipment;

		public virtual PaidRentEquipment PaidRentEquipment {
			get { return paidRentEquipment; }
			set { SetField(ref paidRentEquipment, value, () => PaidRentEquipment); }
		}

		#endregion

		#region Вычисляемы

		/// <summary>
		/// Получает количество оборудования для аренды
		/// </summary>
		int RentEquipmentCount {
			get {
				if(AdditionalAgreement?.Type == AgreementType.NonfreeRent && PaidRentEquipment != null) {
					return PaidRentEquipment.Count;
				}
				if(AdditionalAgreement?.Type == AgreementType.FreeRent && FreeRentEquipment != null) {
					return FreeRentEquipment.Count;
				}
				return 0;
			}
		}

		public virtual bool CanShowReturnedCount {
			get {
				return (Order.OrderStatus >= OrderStatus.OnTheWay
						&& ReturnedCount > 0
						&& Nomenclature.GetCategoriesForShipment().Contains(Nomenclature.Category));
			}
		}

		public virtual bool IsRentCategory {
			get {
				return !IsRentRenewal() && 
					(Nomenclature.Category == NomenclatureCategory.rent
				                            || (RentEquipmentCount > 0));
			}
		}

		public virtual bool IsDepositCategory {
			get {
				return Nomenclature.Category == NomenclatureCategory.deposit;
			}
		}

		int RentTime {
			get {
				if(AdditionalAgreement == null) {
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
				}

				return 0;
			}
		}

		public virtual string RentString {
			get {
				int rentCount = RentTime;
				int count = RentEquipmentCount;

				if(rentCount != 0 && Nomenclature.Category == NomenclatureCategory.rent) {
					return String.Format("{0}*{1}", count, rentCount);
				} else {
					return "";
				}
			}
		}


		public virtual int ReturnedCount {
			get {
				return Count - ActualCount;
			}
		}

		public virtual bool IsDelivered {
			get {
				return ReturnedCount == 0;
			}
		}

		public virtual decimal DiscountForPreview{
			get{
				if(IsDiscountInMoney)
					return DiscountMoney;
				else
					return Discount;
			}
			set{
				CalculateAndSetDiscount(value);
			}
		}

		private void RecalculateDiscount()
		{
			if(!NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(DiscountMoney))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Discount))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Price))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Count))
			   || (Order == null || !NHibernate.NHibernateUtil.IsInitialized(Order.OrderItems))) {
				return;
			}
			if(DiscountMoney > 0) {
				CalculateAndSetDiscount(DiscountForPreview);
			}
		}

		private void CalculateAndSetDiscount(decimal value) 
		{
			if(IsDiscountInMoney) {
				DiscountMoney = value > Price * CurrentCount ? Price * CurrentCount : value;
				Discount = (100 * DiscountMoney) / (Price * CurrentCount);
			} else {
				Discount = value > 100 ? 100 : value;
				DiscountMoney = Price * CurrentCount * Discount / 100;
			}
		}

		/// <summary>
		/// Свойство возвращает подходяшее значение Count или ActualCount в зависимости от статуса заказа.
		/// </summary>
		public int CurrentCount{
			get{
				if(Order != null && OrderRepository.GetStatusesForActualCount(Order).Contains(Order.OrderStatus))
					return ActualCount;
				else
					return Count;
			}
		}

		public virtual decimal Sum => Price * Count - DiscountMoney;//FIXME Count -- CurrentCount

		public virtual decimal ActualSum => Price * CurrentCount - DiscountMoney;

		public virtual bool CanEditAmount {
			get {
				bool result = false;
				if(AdditionalAgreement == null) 
					result = true;
				
				if(AdditionalAgreement?.Type == AgreementType.WaterSales) 
					result = true;
				
				if(Nomenclature.Category == NomenclatureCategory.rent)
					result = false;
				
				if(IsRentRenewal())
					result = true;
				if(Nomenclature.Id == int.Parse(MainSupport.BaseParameters.All["paid_delivery_nomenclature_id"])) {
					result = false;
				}

				return result;
			}
		}

		public virtual string NomenclatureString {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string AgreementString {
			get {
				if(AdditionalAgreement == null) {
					return String.Empty;
				}
				string result = String.Format("{0} №{1}", AdditionalAgreement.AgreementTypeTitle, AdditionalAgreement.FullNumberText);
				if(AdditionalAgreement.Self is FreeRentAgreement && FreeRentEquipment != null) {
					result += string.Format(" Пакет №{0}", FreeRentEquipment.FreeRentPackage.Id);
				}
				return result;
			}
		}

		public virtual string Title {
			get {
				return $"[{Order.Title}] {Nomenclature.Name} - {Count}*{Price}={Sum}";
			}
		}
		#endregion

		#region Методы

		public virtual bool CanEditPrice()
		{
			if(IsRentRenewal()) {
				return true;
			}

			return Nomenclature.GetCategoriesWithEditablePrice().Contains(Nomenclature.Category);
		}

		//FIXME Для предварительной реализации продления аренды пока не решили как она будет работать
		private bool IsRentRenewal()
		{
			if(Order.IsLoadedFrom1C) {
				//Так как все товары в таких заказах не будут привязаны к доп соглашениям
				return false;
			}
			//Определяет что аренда на продажу не связана с дополнительным соглашением и таким образом является 
			//продлением существующей аренды, на КОТОРУЮ ПОКА НЕТ ССЫЛКИ
			return Nomenclature.Category == NomenclatureCategory.rent && AdditionalAgreement == null;
		}

		public virtual decimal? GetWaterFixedPrice()
		{
			decimal? result = null;

			if(Order.IsLoadedFrom1C) {
				return result;
			}

			//влияющая номенклатура
			Nomenclature infuentialNomenclature = Nomenclature?.DependsOnNomenclature;
			if(Nomenclature.Category == NomenclatureCategory.water) {
				var waterSalesAgreement = AdditionalAgreement.Self as WaterSalesAgreement;
				if(waterSalesAgreement == null) {
					return result;
				}
				if(waterSalesAgreement.HasFixedPrice && waterSalesAgreement.FixedPrices.Any(x => x.Nomenclature.Id == Nomenclature.Id
																						   && infuentialNomenclature == null)) {
					result = waterSalesAgreement.FixedPrices.First(x => x.Nomenclature.Id == Nomenclature.Id).Price;
				} else if(waterSalesAgreement.HasFixedPrice && waterSalesAgreement.FixedPrices.Any(x => x.Nomenclature.Id == infuentialNomenclature?.Id)) {
					result = waterSalesAgreement.FixedPrices.First(x => x.Nomenclature.Id == infuentialNomenclature.Id).Price;
				}
			}
			return result;
		}

		public virtual void RecalculatePrice()
		{
			if(IsUserPrice) {
				return;
			}
			Price = GetPriceByTotalCount();
		}

		private Decimal GetPriceByTotalCount()
		{
			if(Nomenclature == null) {
				return 0m;
			}
			if(Nomenclature.DependsOnNomenclature == null) {
				if(Nomenclature.IsWater19L) {
					return Nomenclature.GetPrice(Order.GetTotalWater19LCount());
				} else {
					return Nomenclature.GetPrice(Count);
				}
			} else {
				if(Nomenclature.IsWater19L) {
					return Nomenclature.DependsOnNomenclature.GetPrice(Order.GetTotalWater19LCount());
				} else {
					return Nomenclature.DependsOnNomenclature.GetPrice(Count);
				}
			}
		}

		public virtual CounterpartyMovementOperation UpdateCounterpartyOperation(IUnitOfWork uow)
		{
			if(ActualCount == 0) {
				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Id > 0) {
					uow.Delete(CounterpartyMovementOperation);
				}
				CounterpartyMovementOperation = null;
				return null;
			}

			if(Nomenclature == null)
				throw new InvalidOperationException("Номенклатура не может быть null");

			if(CounterpartyMovementOperation == null) {
				CounterpartyMovementOperation = new CounterpartyMovementOperation {
					Nomenclature = Nomenclature,
					OperationTime = Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
					Amount = ActualCount,
					Equipment = Equipment,
					IncomingCounterparty = Order.Client,
					IncomingDeliveryPoint = Order.DeliveryPoint,
				};
			}

			return CounterpartyMovementOperation;
		}

		public virtual void DeleteAdditionalAgreement(IUnitOfWork uow)
		{
			uow.Delete(this.AdditionalAgreement);
			this.AdditionalAgreement = null;
			uow.Save();
		}

		public virtual void DeleteFreeRentEquipment(IUnitOfWork uow)
		{
			if(this.AdditionalAgreement is FreeRentAgreement) {
				((FreeRentAgreement)this.AdditionalAgreement).RemoveEquipment(this.FreeRentEquipment);
			}

			uow.Delete(this.FreeRentEquipment);
			this.FreeRentEquipment = null;
			uow.Save();
		}

		public virtual void DeletePaidRentEquipment(IUnitOfWork uow)
		{
			if(this.AdditionalAgreement is DailyRentAgreement) {
				((DailyRentAgreement)this.AdditionalAgreement).RemoveEquipment(this.PaidRentEquipment);
			}

			if(this.AdditionalAgreement is NonfreeRentAgreement) {
				((NonfreeRentAgreement)this.AdditionalAgreement).RemoveEquipment(this.PaidRentEquipment);
			}

			uow.Delete(this.PaidRentEquipment);
			this.PaidRentEquipment = null;
			uow.Save();
		}

		#endregion

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			return null;
		}

		#endregion

		#region Внутрение

		void RecalculateNDS()
		{
			decimal s = ActualSum > 0 && ActualSum != Sum ? ActualSum : Sum;
			if(ValueAddedTax.HasValue && s >= 0)
				IncludeNDS = Math.Round(s * ValueAddedTax.Value / (1 + ValueAddedTax.Value), 2);
		}

		#endregion
	}
}

