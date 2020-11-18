using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Parameters;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки заказа",
		Nominative = "строка заказа")]
	[HistoryTrace]
	public class OrderItem : PropertyChangedBase, IDomainObject, IOrderItemWageCalculationSource
	{
		private int? paidDeliveryNomenclatureId;
		private int PaidDeliveryNomenclatureId => paidDeliveryNomenclatureId ?? (paidDeliveryNomenclatureId = int.Parse(ParametersProvider.Instance.GetParameterValue("paid_delivery_nomenclature_id"))).Value;

		#region Свойства

		public virtual int Id { get; set; }

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set {
				if(SetField(ref order, value, () => Order))
					RecalculateNDS();
			}
		}

		AdditionalAgreement additionalAgreement;

		[Display(Name = "Дополнительное соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get => additionalAgreement;
			set => SetField(ref additionalAgreement, value, () => AdditionalAgreement);
		}

		Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
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
			get => equipment;
			set => SetField(ref equipment, value, () => Equipment);
		}

		private bool priceIsSetted = false;

		decimal price;

		[Display(Name = "Цена")]
		public virtual decimal Price {
			get => price;
			set {
				if(priceIsSetted && Order.OrderStatus != OrderStatus.NewOrder)
					return;
				//Если цена не отличается от той которая должна быть по прайсам в 
				//номенклатуре, то цена не изменена пользователем и сможет расчитываться автоматически
				IsUserPrice = value != GetPriceByTotalCount() && value != 0;
				if(IsUserPrice)
					IsUserPrice = value != GetPriceByTotalCount() && value != 0;

				if(SetField(ref price, value, () => Price)) {
					if(AdditionalAgreement?.Self is SalesEquipmentAgreement aa)
						aa.UpdatePrice(Nomenclature, value);
					RecalculateDiscount();
					RecalculateNDS();
				}

				priceIsSetted = true;
			}
		}

		bool isUserPrice;

		[Display(Name = "Цена установлена пользователем")]
		public virtual bool IsUserPrice {
			get => isUserPrice;
			set => SetField(ref isUserPrice, value, () => IsUserPrice);
		}

		decimal count = -1;
		[Display(Name = "Количество")]
		public virtual decimal Count {
			get => count;
			set {
				if(SetField(ref count, value)) {
					if(AdditionalAgreement?.Self is SalesEquipmentAgreement aa)
						aa.UpdateCount(Nomenclature, (int)value);
					Order?.RecalculateItemsPrice();
					RecalculateDiscount();
					RecalculateNDS();
				}
			}
		}

		decimal? actualCount;
		public virtual decimal? ActualCount {
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
			set => SetField(ref includeNDS, value, () => IncludeNDS);
		}

		private bool isDiscountInMoney;

		[Display(Name = "Скидка деньгами?")]
		public virtual bool IsDiscountInMoney {
			get => isDiscountInMoney;
			set {
				if(SetField(ref isDiscountInMoney, value, () => IsDiscountInMoney))
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
				if(SetField(ref discount, value, () => Discount)) {
					RecalculateNDS();
				}
			}
		}

		private decimal? originalDiscount;

		[Display(Name = "Процент скидки на товар которая была установлена до отмены заказа")]
		public virtual decimal? OriginalDiscount {
			get => originalDiscount;
			set { SetField(ref originalDiscount, value, () => OriginalDiscount); } 
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
				if(SetField(ref discountMoney, value, () => DiscountMoney))
					RecalculateNDS();
			}
		}

		private decimal? originalDiscountMoney;
		[Display(Name = "Скидки на товар которая была установлена до отмены заказа")]
		public virtual decimal? OriginalDiscountMoney {
			get => originalDiscountMoney;
			set => SetField(ref originalDiscountMoney, value, () => OriginalDiscountMoney);
		}

		private decimal discountByStock;
		[Display(Name = "Скидка по акции")]
		public virtual decimal DiscountByStock {
			get => discountByStock;
			set => SetField(ref discountByStock, value, () => DiscountByStock);
		}


		decimal? valueAddedTax;
		[Display(Name = "НДС на момент создания заказа")]
		public virtual decimal? ValueAddedTax {
			get => valueAddedTax;
			set => SetField(ref valueAddedTax, value, () => ValueAddedTax);
		}

		private DiscountReason discountReason;

		[Display(Name = "Основание скидки на товар")]
		public virtual DiscountReason DiscountReason {
			get => discountReason;
			set => SetField(ref discountReason, value, () => DiscountReason);
		}

		private DiscountReason originalDiscountReason;
		[Display(Name = "Основание скидки на товар до отмены заказа")]
		public virtual DiscountReason OriginalDiscountReason {
			get => originalDiscountReason;
			set => SetField(ref originalDiscountReason, value, () => OriginalDiscountReason);
		}


		CounterpartyMovementOperation counterpartyMovementOperation;

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation {
			get => counterpartyMovementOperation;
			set => SetField(ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation);
		}

		FreeRentEquipment freeRentEquipment;

		public virtual FreeRentEquipment FreeRentEquipment {
			get => freeRentEquipment;
			set => SetField(ref freeRentEquipment, value, () => FreeRentEquipment);
		}

		PaidRentEquipment paidRentEquipment;

		public virtual PaidRentEquipment PaidRentEquipment {
			get => paidRentEquipment;
			set => SetField(ref paidRentEquipment, value, () => PaidRentEquipment);
		}

		PromotionalSet promoSet;
		[Display(Name = "Добавлено из промо-набора")]
		public virtual PromotionalSet PromoSet {
			get => promoSet;
			set => SetField(ref promoSet, value, () => PromoSet);
		}

		#endregion

		#region Вычисляемые

		/// <summary>
		/// Получает количество оборудования для аренды
		/// </summary>
		int RentEquipmentCount {
			get {
				if(AdditionalAgreement?.Type == AgreementType.NonfreeRent && PaidRentEquipment != null)
					return PaidRentEquipment.Count;

				if(AdditionalAgreement?.Type == AgreementType.FreeRent && FreeRentEquipment != null)
					return FreeRentEquipment.Count;

				return 0;
			}
		}

		public virtual bool CanShowReturnedCount => Order.OrderStatus >= OrderStatus.OnTheWay && ReturnedCount > 0
														&& Nomenclature.GetCategoriesForShipment().Contains(Nomenclature.Category);

		public virtual bool IsRentCategory => !IsRentRenewal() && RentEquipmentCount > 0;

		public virtual bool IsDepositCategory => Nomenclature.Category == NomenclatureCategory.deposit;

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

				if(rentCount != 0)
					return string.Format("{0}*{1}", count, rentCount);
				return string.Empty;
			}
		}

		public virtual decimal ReturnedCount => Count - ActualCount ?? 0;

		public virtual bool IsDelivered => ReturnedCount == 0;

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

		public virtual decimal ManualChangingOriginalDiscount {
			get	=> IsDiscountInMoney ? (OriginalDiscountMoney ?? 0) : (OriginalDiscount ?? 0);
		}

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
			   || (Order == null || !NHibernate.NHibernateUtil.IsInitialized(Order.OrderItems))) {
				return;
			}

			if(CurrentCount == 0)
				RemoveDiscount();
			else
				CalculateAndSetDiscount(DiscountSetter);
		}

		void RemoveDiscount()
		{
			if(DiscountMoney > 0) {
				OriginalDiscountMoney = DiscountMoney;
				OriginalDiscountReason = DiscountReason;
				OriginalDiscount = Discount;
			}
			DiscountReason = null;
			DiscountMoney = 0;
			Discount = 0;
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

		private decimal GetPercentDiscount() => IsDiscountInMoney ? (100 * DiscountMoney) / (Price * CurrentCount) : Discount;

		public void SetDiscountByStock(DiscountReason discountReasonForStockBottle, decimal discountPercent)
		{
			//ограничение на значения только от 0 до 100
			discountPercent = discountPercent > 100 ? 100 : discountPercent < 0 ? 0 : discountPercent;

			var existingPercent = GetPercentDiscount();
			if(existingPercent == 100 && DiscountByStock == 0) {
				return;
			}

			decimal originalExistingPercent = existingPercent - DiscountByStock;

			decimal resultDiscount = originalExistingPercent + discountPercent;
			resultDiscount = resultDiscount > 100 ? 100 : resultDiscount < 0 ? 0 : resultDiscount;

			//на сколько избыточна добавляемая скидка (значение отрицательное)
			decimal discountExcess = (originalExistingPercent + discountPercent > 100) ? (100 - (originalExistingPercent + discountPercent)) : 0;
			decimal realDiscountByStock = discountPercent + discountExcess;


			Discount = resultDiscount;
			DiscountMoney = Price * CurrentCount * Discount / 100;
			DiscountByStock = realDiscountByStock;

			if(Discount == 0) {
				DiscountReason = null;
			} else if(DiscountReason == null) {
				DiscountReason = discountReasonForStockBottle;
			}
		}

		public decimal CurrentCount => ActualCount ?? Count;

		public virtual decimal Sum => Price * Count - DiscountMoney;//FIXME Count -- CurrentCount

		public virtual decimal ActualSum => Price * CurrentCount - DiscountMoney;

		public virtual decimal OriginalSum => Price * Count - (OriginalDiscountMoney ?? 0);

		public virtual bool CanEditAmount {
			get {
				bool result = AdditionalAgreement == null;

				if(AdditionalAgreement?.Type == AgreementType.WaterSales)
					result = true;

				if(AdditionalAgreement?.Type == AgreementType.EquipmentSales)
					result = true;

				if(IsRentRenewal())
					result = true;

				if(Nomenclature.Id == PaidDeliveryNomenclatureId)
					result = false;

				if(PromoSet != null && !PromoSet.CanEditNomenclatureCount)
					result = false;

				return result;
			}
		}

		public virtual bool CanEditPrice {
			get {
				if(PromoSet != null) {
					return false;
				}
				if(IsRentRenewal())
					return true;

				return Nomenclature.GetCategoriesWithEditablePrice().Contains(Nomenclature.Category);
			}
		}

		public virtual string NomenclatureString => Nomenclature != null ? Nomenclature.Name : string.Empty;

		public virtual string AgreementString {
			get {
				if(AdditionalAgreement != null) {
					string result = string.Format("{0} №{1}", AdditionalAgreement.AgreementTypeTitle, AdditionalAgreement.FullNumberText);
					if(AdditionalAgreement.Self is FreeRentAgreement && FreeRentEquipment != null)
						result += string.Format(" Пакет №{0}", FreeRentEquipment.FreeRentPackage.Id);
					return result;
				}
				return string.Empty;
			}
		}

		public virtual string Title => $"[{Order.Title}] {Nomenclature.Name} - {Count}*{Price}={Sum}";

		#region IOrderItemWageCalculationSource implementation

		public virtual decimal InitialCount => Count;

		public virtual decimal PercentForMaster => (decimal)Nomenclature.PercentForMaster;

		public virtual bool IsMasterNomenclature => Nomenclature.Category == NomenclatureCategory.master;

		#endregion IOrderItemWageCalculationSource implementation

		#endregion

		#region Методы

		//FIXME Для предварительной реализации продления аренды пока не решили как она будет работать
		private bool IsRentRenewal()
		{
			if(Order.IsLoadedFrom1C)
				//Так как все товары в таких заказах не будут привязаны к доп соглашениям
				return false;

			//Определяет что аренда на продажу не связана с дополнительным соглашением и таким образом является 
			//продлением существующей аренды, на КОТОРУЮ ПОКА НЕТ ССЫЛКИ
			return AdditionalAgreement == null;
		}

		public virtual decimal? GetWaterFixedPrice()
		{
			decimal? result = null;

			if(Order.IsLoadedFrom1C)
				return result;

			//влияющая номенклатура
			Nomenclature infuentialNomenclature = Nomenclature?.DependsOnNomenclature;
			if(Nomenclature.Category == NomenclatureCategory.water) {
				if(!(AdditionalAgreement.Self is WaterSalesAgreement waterSalesAgreement))
					return result;

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
			if(IsUserPrice || PromoSet != null)
				return;

			Price = GetPriceByTotalCount();
		}

		public virtual decimal GetPriceByTotalCount()
		{
			if(Nomenclature != null) {
				if(Nomenclature.DependsOnNomenclature == null)
					return Nomenclature.GetPrice(Nomenclature.IsWater19L ? Order.GetTotalWater19LCount(doNotCountWaterFromPromoSets: true) : Count);
				if(Nomenclature.IsWater19L)
					return Nomenclature.DependsOnNomenclature.GetPrice(Nomenclature.IsWater19L ? Order.GetTotalWater19LCount(doNotCountWaterFromPromoSets: true) : Count);
			}
			return 0m;
		}

		public virtual CounterpartyMovementOperation UpdateCounterpartyOperation(IUnitOfWork uow)
		{
			if(!ActualCount.HasValue || ActualCount.Value == 0) {
				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Id > 0)
					uow.Delete(CounterpartyMovementOperation);

				CounterpartyMovementOperation = null;
				return null;
			}

			if(Nomenclature == null)
				throw new InvalidOperationException("Номенклатура не может быть null");

			if(CounterpartyMovementOperation == null) {
				CounterpartyMovementOperation = new CounterpartyMovementOperation {
					Nomenclature = Nomenclature,
					OperationTime = Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
					Amount = ActualCount.Value,//не могу представить ситуацию с null - пусть будет exception если что
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
			if(this.AdditionalAgreement is DailyRentAgreement)
				((DailyRentAgreement)this.AdditionalAgreement).RemoveEquipment(this.PaidRentEquipment);

			if(this.AdditionalAgreement is NonfreeRentAgreement)
				((NonfreeRentAgreement)this.AdditionalAgreement).RemoveEquipment(this.PaidRentEquipment);

			uow.Delete(this.PaidRentEquipment);
			this.PaidRentEquipment = null;
			uow.Save();
		}

		#endregion

		#region Внутрение

		void RecalculateNDS()
		{
			if(ValueAddedTax.HasValue)
				IncludeNDS = Math.Round(ActualSum * ValueAddedTax.Value / (1 + ValueAddedTax.Value), 2);
		}

		#endregion
	}
}

