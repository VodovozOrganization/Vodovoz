﻿using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using System.Linq;
using QSHistoryLog;

namespace Vodovoz.Domain.Orders
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки заказа",
		Nominative = "строка заказа")]
	public class OrderItem: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		public virtual int Id { get; set; }

		Order order;

		[Display (Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}
			
		AdditionalAgreement additionalAgreement;

		[Display (Name = "Дополнительное соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get { return additionalAgreement; }
			set { SetField (ref additionalAgreement, value, () => AdditionalAgreement); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		Decimal price;

		[Display (Name = "Цена")]
		public virtual Decimal Price {
			get { return price; }
			set { 
				if(SetField (ref price, value, () => Price))
				{
					RecalculateNDS();
				}
			}
		}

		int count=-1;

		[Display (Name = "Количество")]
		public virtual int Count {
			get {
				return count; 
			}
			set {
				if (count != -1 && !IsRentCategory) {	
					var oldDefaultPrice = DefaultPrice;
					var newDefaultPrice = GetDefaultPrice (value);
					if (Price == oldDefaultPrice)
						Price = newDefaultPrice;
					DefaultPrice = newDefaultPrice;
				}
				if(SetField (ref count, value, () => Count))
				{
					RecalculateNDS();
				}
			}
		}

		int actualCount;
		public virtual int ActualCount{
			get{
				return actualCount;
			}
			set{
				SetField(ref actualCount, value, () => ActualCount);
			}
		}

		Decimal includeNDS;

		[Display (Name = "Включая НДС")]
		public virtual Decimal IncludeNDS {
			get { return includeNDS; }
			set { SetField (ref includeNDS, value, () => IncludeNDS); }
		}

		private int discount;

		[Display(Name = "Процент скидки на товар")]
		public virtual int Discount
		{
			get { return discount; }
			set 
			{
				if(value != discount && value == 0) {
					DiscountReason = null;
				}
				if(SetField(ref discount, value, () => Discount)){
					RecalculateNDS();
				} 
			}
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
			set { SetField (ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation); }
		}

		FreeRentEquipment freeRentEquipment;

		public virtual FreeRentEquipment FreeRentEquipment {
			get { return freeRentEquipment; }
			set { SetField(ref freeRentEquipment, value, () => FreeRentEquipment); }
		}

		PaidRentEquipment paidRentEquipment;

		public virtual PaidRentEquipment PaidRentEquipment
		{
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

		public virtual bool CanShowReturnedCount{
			get {
				return (Order.OrderStatus >= OrderStatus.OnTheWay
				        && ReturnedCount > 0
				        && Nomenclature.GetCategoriesForShipment().Contains(Nomenclature.Category));
			}
		}

		public virtual bool IsRentCategory {
			get {
				return Nomenclature.Category == NomenclatureCategory.rent
								   || (RentEquipmentCount > 0);
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


		public virtual int ReturnedCount{
			get{
				return Count - ActualCount;
			}
		}

		public virtual bool IsDelivered{
			get{
				return ReturnedCount == 0;
			}
		}

		protected Decimal GetDefaultPrice(int count){
			Decimal result=0;
			result = Nomenclature.GetPrice(count);
			if (Nomenclature.Category == NomenclatureCategory.water) {
				var waterSalesAgreement = AdditionalAgreement as WaterSalesAgreement;
				if (waterSalesAgreement != null && waterSalesAgreement.IsFixedPrice
					&& waterSalesAgreement.FixedPrices.Any(x => x.Nomenclature.Id == Nomenclature.Id))
					result = waterSalesAgreement.FixedPrices.First(x => x.Nomenclature.Id == Nomenclature.Id).Price;
			}
			return result;
		}

		Decimal defaultPrice=-1;

		[IgnoreHistoryTrace]
		public virtual Decimal DefaultPrice {
			get { 
				if (defaultPrice == -1) {
					defaultPrice = GetDefaultPrice (count);
				}
				return defaultPrice; 
			}
			set { SetField (ref defaultPrice, value, () => DefaultPrice); }
		}

		public virtual bool HasUserSpecifiedPrice(){
			return price != DefaultPrice;
		}

		public virtual decimal Sum{
			get{
				return Price * Count * (1 - (decimal)Discount/100) ;
			}
		}

		public virtual bool CanEditAmount {
			get {
				bool result = false;
				if(AdditionalAgreement == null) {
					result = true;
				}
				if(AdditionalAgreement?.Type == AgreementType.WaterSales
				   || AdditionalAgreement?.Type == AgreementType.NonfreeRent
				   || AdditionalAgreement?.Type == AgreementType.DailyRent) {
					result = true;
				}
				if(Nomenclature.Category == NomenclatureCategory.rent
				   || Nomenclature.Category == NomenclatureCategory.deposit) {
					result = false;
				}

				return result;
			}
		}

		public virtual string NomenclatureString {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string AgreementString {
			get { return AdditionalAgreement == null ? String.Empty : String.Format ("{0} №{1}", AdditionalAgreement.AgreementTypeTitle, AdditionalAgreement.AgreementNumber); }
		}

		public virtual string Title {
			get {
				return $"[{order.Title}] {Nomenclature.Name} - {Count}*{Price}={Sum}";
			}
		}
		#endregion

		#region Функции


		public virtual CounterpartyMovementOperation UpdateCounterpartyOperation(IUnitOfWork uow)
		{
			if(ActualCount == 0)
			{
				if (CounterpartyMovementOperation != null && CounterpartyMovementOperation.Id > 0)
				{
					uow.Delete (CounterpartyMovementOperation);
				}
				CounterpartyMovementOperation = null;
				return null;
			}

			if (Nomenclature == null)
				throw new InvalidOperationException ("Номенклатура не может быть null");
		
			if (CounterpartyMovementOperation == null) {
				CounterpartyMovementOperation = new CounterpartyMovementOperation {
					Nomenclature = Nomenclature,
					OperationTime = Order.DeliveryDate.Value.Date.AddHours (23).AddMinutes (59),
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
			if (this.AdditionalAgreement is FreeRentAgreement)
			{
				((FreeRentAgreement)this.AdditionalAgreement).RemoveEquipment(this.FreeRentEquipment);
			}

			uow.Delete(this.FreeRentEquipment);
			this.FreeRentEquipment = null;
			uow.Save();
		}

		public virtual void DeletePaidRentEquipment(IUnitOfWork uow)
		{
			if (this.AdditionalAgreement is DailyRentAgreement)
			{
				((DailyRentAgreement)this.AdditionalAgreement).RemoveEquipment(this.PaidRentEquipment);
			}

			if (this.AdditionalAgreement is NonfreeRentAgreement)
			{
				((NonfreeRentAgreement)this.AdditionalAgreement).RemoveEquipment(this.PaidRentEquipment);
			}

			uow.Delete(this.PaidRentEquipment);
			this.PaidRentEquipment = null;
			uow.Save();
		}

		#endregion
	
		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion

		#region Внутрение

		private void RecalculateNDS()
		{
			if (Nomenclature == null || Sum < 0)
				return;

			switch (Nomenclature.VAT)
			{
				case VAT.Vat18:
					IncludeNDS = Math.Round(Sum - (Sum / 1.18m), 2);
					break;
				case VAT.Vat10:
					IncludeNDS = Math.Round(Sum - (Sum / 1.10m), 2);
					break;
				default:
					break;
			}
		}

		#endregion
	}
}

