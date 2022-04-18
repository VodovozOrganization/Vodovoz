using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate;
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
	public class OrderItem : PropertyChangedBase, IDomainObject, IOrderItemWageCalculationSource, IDiscount
	{
		private NomenclatureParametersProvider _nomenclatureParameterProvider = new NomenclatureParametersProvider(new ParametersProvider());

		private int? paidDeliveryNomenclatureId;
		private int PaidDeliveryNomenclatureId =>
			paidDeliveryNomenclatureId ?? (paidDeliveryNomenclatureId = _nomenclatureParameterProvider.PaidDeliveryNomenclatureId).Value;

		private int? _fastDeliveryNomenclatureId;
		private int FastDeliveryNomenclatureId =>
			_fastDeliveryNomenclatureId ?? (_fastDeliveryNomenclatureId = _nomenclatureParameterProvider.FastDeliveryNomenclatureId).Value;

		#region Свойства

		public virtual int Id { get; set; }

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set {
				if(SetField(ref order, value, () => Order))
					RecalculateVAT();
			}
		}

		Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set {
				if(SetField(ref nomenclature, value, () => Nomenclature)) {
					CalculateVATType();
				}
			}
		}

		Equipment equipment;

		[Display(Name = "Оборудование")]
		public virtual Equipment Equipment {
			get => equipment;
			set => SetField(ref equipment, value, () => Equipment);
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

				if(SetField(ref price, value, () => Price)) {
					RecalculateDiscount();
					RecalculateVAT();
				}
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
				if(Nomenclature?.Unit?.Digits == 0 && value % 1 != 0)
					value = Math.Truncate(value);
				if(SetField(ref count, value)) {
					Order?.RecalculateItemsPrice();
					RecalculateDiscount();
					RecalculateVAT();
					Order?.UpdateRentsCount();
				}
			}
		}

		decimal? actualCount;
		public virtual decimal? ActualCount {
			get => actualCount;
			set {
				if(SetField(ref actualCount, value)) {
					RecalculateDiscount();
					RecalculateVAT();
				}
			}
		}

		decimal? includeNDS;

		[Display(Name = "Включая НДС")]
		public virtual decimal? IncludeNDS {
			get => includeNDS;
			set => SetField(ref includeNDS, value, () => IncludeNDS);
		}

		private bool isDiscountInMoney;

		[Display(Name = "Скидка деньгами?")]
		public virtual bool IsDiscountInMoney {
			get => isDiscountInMoney;
			set {
				if(SetField(ref isDiscountInMoney, value, () => IsDiscountInMoney))
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
				if(SetField(ref discount, value, () => Discount)) {
					RecalculateVAT();
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
				if(value != discountMoney && value == 0) {
					DiscountReason = null;
				}
				if(SetField(ref discountMoney, value, () => DiscountMoney))
					RecalculateVAT();
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

		PromotionalSet promoSet;
		[Display(Name = "Добавлено из промо-набора")]
		public virtual PromotionalSet PromoSet {
			get => promoSet;
			set => SetField(ref promoSet, value, () => PromoSet);
		}

		#region Аренда

		OrderRentType rentType;
		[Display(Name = "Тип аренды")]
		public virtual OrderRentType RentType {
			get => rentType;
			set => SetField(ref rentType, value);
		}
		
		OrderItemRentSubType orderItemRentSubType;
		[Display(Name = "Подтип позиции аренды")]
		public virtual OrderItemRentSubType OrderItemRentSubType {
			get => orderItemRentSubType;
			set => SetField(ref orderItemRentSubType, value);
		}

		int rentCount;
		[Display(Name = "Количество аренды (дни/месяцы)")]
		public virtual int RentCount {
			get => rentCount;
			set {
				if (SetField(ref rentCount, value)) {
					Order?.UpdateRentsCount();
				}
			}
		}

		int rentEquipmentCount;
		[Display(Name = "Количество оборудования для аренды")]
		public virtual int RentEquipmentCount {
			get => rentEquipmentCount;
			set => SetField(ref rentEquipmentCount, value);
		}
		
		PaidRentPackage paidRentPackage;
		[Display(Name = "Пакет платной аренды")]
		public virtual PaidRentPackage PaidRentPackage {
			get => paidRentPackage;
			set => SetField(ref paidRentPackage, value);
		}
		
		FreeRentPackage freeRentPackage;
		[Display(Name = "Пакет бесплатной аренды")]
		public virtual FreeRentPackage FreeRentPackage {
			get => freeRentPackage;
			set => SetField(ref freeRentPackage, value);
		}
		
		public virtual void SetRentEquipmentCount(int equipmentCount)
		{
			RentEquipmentCount = equipmentCount;
			switch (OrderItemRentSubType) {
				case OrderItemRentSubType.RentServiceItem:
					Count = RentCount * RentEquipmentCount;
					break;
				case OrderItemRentSubType.RentDepositItem:
					Count = RentEquipmentCount;
					break;
			}
		}

		#endregion Аренда

		#endregion

		#region Вычисляемые

		public virtual bool CanShowReturnedCount => Order.OrderStatus >= OrderStatus.OnTheWay && ReturnedCount > 0
														&& Nomenclature.GetCategoriesForShipment().Contains(Nomenclature.Category);

		public virtual bool IsDepositCategory => Nomenclature.Category == NomenclatureCategory.deposit;

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
			if(existingPercent == 100 && DiscountByStock == 0)
			{
				return;
			}

			decimal originalExistingPercent = 100 * (existingPercent - DiscountByStock) / (100 - DiscountByStock);

			decimal resultDiscount = originalExistingPercent + (100 - originalExistingPercent) / 100 * discountPercent;

			Discount = resultDiscount;
			DiscountMoney = Price * CurrentCount * Discount / 100;
			DiscountByStock = discountPercent;

			if(Discount == 0)
			{
				DiscountReason = null;
			}
			else if((DiscountReason == null && PromoSet == null) || (DiscountReason == null && PromoSet != null && existingPercent == 0))
			{
				DiscountReason = discountReasonForStockBottle;
			}
		}

		public decimal CurrentCount => ActualCount ?? Count;

		public virtual decimal Sum => Math.Round(Price * Count - DiscountMoney, 2);

		public virtual decimal ActualSum => Math.Round(Price * CurrentCount - DiscountMoney, 2);

		public virtual decimal OriginalSum => Math.Round(Price * Count - (OriginalDiscountMoney ?? 0), 2);

		public virtual bool CanEditAmount {
			get {
				bool result = true;

				if(RentType != OrderRentType.None) {
					result = false;
				}

				if(Nomenclature.Id == PaidDeliveryNomenclatureId)
					result = false;

				if(PromoSet != null && !PromoSet.CanEditNomenclatureCount)
					result = false;

				if(Nomenclature.Id == FastDeliveryNomenclatureId)
				{
					result = false;
				}

				return result;
			}
		}

		public virtual bool CanEditPrice {
			get {
				if(PromoSet != null) {
					return false;
				}
				
				if(RentType != OrderRentType.None) {
					return false;
				}

				return Nomenclature.GetCategoriesWithEditablePrice().Contains(Nomenclature.Category);
			}
		}

		public virtual bool RentVisible => OrderItemRentSubType == OrderItemRentSubType.RentServiceItem;

		public virtual string NomenclatureString => Nomenclature != null ? Nomenclature.Name : string.Empty;

		public virtual string Title => $"[{Order.Title}] {Nomenclature.Name} - {Count}*{Price}={Sum}";

		#region IOrderItemWageCalculationSource implementation

		public virtual decimal InitialCount => Count;

		public virtual decimal PercentForMaster => (decimal)Nomenclature.PercentForMaster;

		public virtual bool IsMasterNomenclature => Nomenclature.Category == NomenclatureCategory.master;

		#endregion IOrderItemWageCalculationSource implementation

		#endregion

		#region Методы

		public virtual decimal? GetWaterFixedPrice()
		{
			decimal? result = null;

			if(Order.IsLoadedFrom1C)
				return result;

			//влияющая номенклатура
			if(Nomenclature.Category == NomenclatureCategory.water) {
				var fixedPrice = order.GetFixedPriceOrNull(Nomenclature);
				if (fixedPrice != null) {
					return fixedPrice.Price;
				}
			}
			return result;
		}

		public virtual void RecalculatePrice()
		{
			if(IsUserPrice || PromoSet != null || Order.OrderStatus == OrderStatus.Closed || order.GetFixedPriceOrNull(Nomenclature) != null)
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

		#endregion

		#region Внутрение

		public virtual void CalculateVATType()
		{
			if(!NHibernateUtil.IsInitialized(Nomenclature)) {
				NHibernateUtil.Initialize(Nomenclature);
			}
			
			if(!NHibernateUtil.IsInitialized(Order)) {
				NHibernateUtil.Initialize(Order);
			}
			
			if(Order == null || Nomenclature == null) {
				return;
			}
			
			VAT vat = CanUseVAT() ? Nomenclature.VAT : VAT.No;
			
			switch(vat) {
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

			RecalculateVAT();
		}
		
		private void RecalculateVAT()
		{
			if(Order == null) {
				return;
			}
			if(!CanUseVAT()) {
				IncludeNDS = null;
				return;
			}
			
			if(CanUseVAT() && ValueAddedTax.HasValue) {
				IncludeNDS = Math.Round(ActualSum * ValueAddedTax.Value / (1 + ValueAddedTax.Value), 2);
			}
		}

		private bool CanUseVAT()
		{
			if(!NHibernateUtil.IsInitialized(Order)) {
				NHibernateUtil.Initialize(Order);
			}

			bool canUseVAT = true;
			if(Order.Contract?.Organization != null) {
				canUseVAT = !Order.Contract.Organization.WithoutVAT;
			}

			return canUseVAT;
		}

		#endregion
	}

	public enum OrderRentType
	{
		[Display(Name = "Нет аренды")]
		None,
		
		[Display(Name = "Долгосрочная аренда")]
		NonFreeRent,
		
		[Display(Name = "Бесплатная аренда")]
		FreeRent,
		
		[Display(Name = "Посуточная аренда")]
		DailyRent
	}
	
	public class OrderRentTypeStringType : NHibernate.Type.EnumStringType
	{
		public OrderRentTypeStringType () : base (typeof(OrderRentType))
		{
		}
	}
	
	public enum OrderItemRentSubType
	{
		[Display(Name = "Нет аренды")]
		None,
		
		[Display(Name = "Услуга аренды")]
		RentServiceItem,
		
		[Display(Name = "Залог за аренду")]
		RentDepositItem
	}
	
	public class OrderItemRentSubTypeStringType : NHibernate.Type.EnumStringType
	{
		public OrderItemRentSubTypeStringType () : base (typeof(OrderItemRentSubType))
		{
		}
	}
}

