using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.PromotionalSets;
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
		private int _id;
		private Order _order;
		private Nomenclature _nomenclature;
		private Equipment _equipment;
		private decimal _price;
		private bool _isUserPrice;
		private decimal _count = -1;
		private decimal? _actualCount;
		private decimal? _includeNDS;
		private bool _isDiscountInMoney;
		private decimal _discount;
		private decimal? _originalDiscount;
		private decimal _discountMoney;
		private decimal? _originalDiscountMoney;
		private decimal _discountByStock;
		private decimal? _valueAddedTax;
		private DiscountReason _discountReason;
		private DiscountReason _originalDiscountReason;
		private CounterpartyMovementOperation _counterpartyMovementOperation;
		private PromotionalSet _promoSet;
		private bool _isAlternativePrice;
		private bool _isFixedPrice;
		private OrderRentType _rentType;
		private OrderItemRentSubType _orderItemRentSubType;
		private int _rentCount;
		private int _rentEquipmentCount;
		private PaidRentPackage _paidRentPackage;
		private FreeRentPackage _freeRentPackage;

		private NomenclatureParametersProvider _nomenclatureParameterProvider = new NomenclatureParametersProvider(new ParametersProvider());

		private int? paidDeliveryNomenclatureId;
		private int PaidDeliveryNomenclatureId =>
			paidDeliveryNomenclatureId ?? (paidDeliveryNomenclatureId = _nomenclatureParameterProvider.PaidDeliveryNomenclatureId).Value;

		private int? _fastDeliveryNomenclatureId;
		private int FastDeliveryNomenclatureId =>
			_fastDeliveryNomenclatureId ?? (_fastDeliveryNomenclatureId = _nomenclatureParameterProvider.FastDeliveryNomenclatureId).Value;

		private OrderItem _copiedFromUndelivery;

		#region Свойства

		public virtual int Id
		{
			get => _id;
			set => _id = value;
		}

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set
			{
				if(SetField(ref _order, value, () => Order))
					RecalculateVAT();
			}
		}

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set
			{
				if(SetField(ref _nomenclature, value, () => Nomenclature))
				{
					CalculateVATType();
				}
			}
		}

		[Display(Name = "Оборудование")]
		public virtual Equipment Equipment
		{
			get => _equipment;
			set => SetField(ref _equipment, value, () => Equipment);
		}

		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set
			{
				//Если цена не отличается от той которая должна быть по прайсам в 
				//номенклатуре, то цена не изменена пользователем и сможет расчитываться автоматически
				IsUserPrice = (value != GetPriceByTotalCount() && value != 0 && !IsFixedPrice) || CopiedFromUndelivery != null;
				if(IsUserPrice)
					IsUserPrice = (value != GetPriceByTotalCount() && value != 0 && !IsFixedPrice) || CopiedFromUndelivery != null;

				if(SetField(ref _price, value, () => Price))
				{
					RecalculateDiscount();
					RecalculateVAT();
				}
			}
		}

		[Display(Name = "Цена установлена пользователем")]
		public virtual bool IsUserPrice
		{
			get => _isUserPrice;
			set => SetField(ref _isUserPrice, value, () => IsUserPrice);
		}

		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			set
			{
				if(Nomenclature?.Unit?.Digits == 0 && value % 1 != 0)
					value = Math.Truncate(value);
				if(SetField(ref _count, value))
				{
					Order?.RecalculateItemsPrice();
					RecalculateDiscount();
					RecalculateVAT();
					Order?.UpdateRentsCount();
				}
			}
		}

		public virtual decimal? ActualCount
		{
			get => _actualCount;
			set
			{
				if(SetField(ref _actualCount, value))
				{
					RecalculateDiscount();
					RecalculateVAT();
				}
			}
		}

		[Display(Name = "Включая НДС")]
		public virtual decimal? IncludeNDS
		{
			get => _includeNDS;
			set => SetField(ref _includeNDS, value, () => IncludeNDS);
		}

		[Display(Name = "Скидка деньгами?")]
		public virtual bool IsDiscountInMoney
		{
			get => _isDiscountInMoney;
			set
			{
				if(SetField(ref _isDiscountInMoney, value, () => IsDiscountInMoney))
					RecalculateVAT();
			}
		}

		[Display(Name = "Процент скидки на товар")]
		public virtual decimal Discount
		{
			get => _discount;
			set
			{
				if(value != _discount && value == 0)
				{
					DiscountReason = null;
				}
				if(SetField(ref _discount, value, () => Discount))
				{
					RecalculateVAT();
				}
			}
		}

		[Display(Name = "Процент скидки на товар которая была установлена до отмены заказа")]
		public virtual decimal? OriginalDiscount
		{
			get => _originalDiscount;
			set { SetField(ref _originalDiscount, value, () => OriginalDiscount); }
		}

		[Display(Name = "Скидка на товар в деньгах")]
		public virtual decimal DiscountMoney
		{
			get => _discountMoney;
			set
			{
				if(value != _discountMoney && value == 0)
				{
					DiscountReason = null;
				}
				if(SetField(ref _discountMoney, value, () => DiscountMoney))
					RecalculateVAT();
			}
		}

		[Display(Name = "Скидки на товар которая была установлена до отмены заказа")]
		public virtual decimal? OriginalDiscountMoney
		{
			get => _originalDiscountMoney;
			set => SetField(ref _originalDiscountMoney, value, () => OriginalDiscountMoney);
		}

		[Display(Name = "Скидка по акции")]
		public virtual decimal DiscountByStock
		{
			get => _discountByStock;
			set => SetField(ref _discountByStock, value, () => DiscountByStock);
		}

		[Display(Name = "НДС на момент создания заказа")]
		public virtual decimal? ValueAddedTax
		{
			get => _valueAddedTax;
			set => SetField(ref _valueAddedTax, value, () => ValueAddedTax);
		}


		[Display(Name = "Основание скидки на товар")]
		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value, () => DiscountReason);
		}

		[Display(Name = "Основание скидки на товар до отмены заказа")]
		public virtual DiscountReason OriginalDiscountReason
		{
			get => _originalDiscountReason;
			set => SetField(ref _originalDiscountReason, value, () => OriginalDiscountReason);
		}

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation
		{
			get => _counterpartyMovementOperation;
			set => SetField(ref _counterpartyMovementOperation, value, () => CounterpartyMovementOperation);
		}

		[Display(Name = "Добавлено из промо-набора")]
		public virtual PromotionalSet PromoSet
		{
			get => _promoSet;
			set => SetField(ref _promoSet, value, () => PromoSet);
		}

		[Display(Name = "Альтернативная цена?")]
		public virtual bool IsAlternativePrice
		{
			get => _isAlternativePrice;
			set => SetField(ref _isAlternativePrice, value);
		}

		[Display(Name = "Установлена фиксированная цена?")]
		public virtual bool IsFixedPrice
		{
			get => _isFixedPrice;
			set => SetField(ref _isFixedPrice, value);
		}

		#region Аренда

		[Display(Name = "Тип аренды")]
		public virtual OrderRentType RentType
		{
			get => _rentType;
			set => SetField(ref _rentType, value);
		}

		[Display(Name = "Подтип позиции аренды")]
		public virtual OrderItemRentSubType OrderItemRentSubType
		{
			get => _orderItemRentSubType;
			set => SetField(ref _orderItemRentSubType, value);
		}

		[Display(Name = "Количество аренды (дни/месяцы)")]
		public virtual int RentCount
		{
			get => _rentCount;
			set
			{
				if(SetField(ref _rentCount, value))
				{
					Order?.UpdateRentsCount();
				}
			}
		}

		[Display(Name = "Количество оборудования для аренды")]
		public virtual int RentEquipmentCount
		{
			get => _rentEquipmentCount;
			set => SetField(ref _rentEquipmentCount, value);
		}

		[Display(Name = "Пакет платной аренды")]
		public virtual PaidRentPackage PaidRentPackage
		{
			get => _paidRentPackage;
			set => SetField(ref _paidRentPackage, value);
		}


		[Display(Name = "Пакет бесплатной аренды")]
		public virtual FreeRentPackage FreeRentPackage
		{
			get => _freeRentPackage;
			set => SetField(ref _freeRentPackage, value);
		}

		public virtual void SetRentEquipmentCount(int equipmentCount)
		{
			RentEquipmentCount = equipmentCount;
			switch(OrderItemRentSubType)
			{
				case OrderItemRentSubType.RentServiceItem:
					Count = RentCount * RentEquipmentCount;
					break;
				case OrderItemRentSubType.RentDepositItem:
					Count = RentEquipmentCount;
					break;
			}
		}

		#endregion Аренда

		public virtual OrderItem CopiedFromUndelivery
		{
			get => _copiedFromUndelivery;
			set => SetField(ref _copiedFromUndelivery, value);
		}

		#endregion

		#region Вычисляемые

		public virtual bool CanShowReturnedCount => Order.OrderStatus >= OrderStatus.OnTheWay && ReturnedCount > 0
														&& Nomenclature.GetCategoriesForShipment().Contains(Nomenclature.Category);

		public virtual bool IsDepositCategory => Nomenclature.Category == NomenclatureCategory.deposit;

		public virtual decimal ReturnedCount => Count - ActualCount ?? 0;

		public virtual bool IsDelivered => ReturnedCount == 0;

		public virtual decimal ManualChangingDiscount
		{
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set
			{
				CalculateAndSetDiscount(value);
				if(DiscountByStock != 0)
				{
					DiscountByStock = 0;
					DiscountReason = null;
				}
			}
		}

		public virtual decimal ManualChangingOriginalDiscount
		{
			get => IsDiscountInMoney ? (OriginalDiscountMoney ?? 0) : (OriginalDiscount ?? 0);
		}

		public virtual decimal DiscountSetter
		{
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set => CalculateAndSetDiscount(value);
		}

		private void RecalculateDiscount()
		{
			if(!NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(DiscountMoney))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Discount))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Price))
			   || (Order == null || !NHibernate.NHibernateUtil.IsInitialized(Order.OrderItems)))
			{
				return;
			}

			if(CurrentCount == 0)
				RemoveDiscount();
			else
				CalculateAndSetDiscount(DiscountSetter);
		}

		private void RemoveDiscount()
		{
			if(DiscountMoney > 0)
			{
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
			if((Price * CurrentCount) == 0)
			{
				DiscountMoney = 0;
				Discount = 0;
				return;
			}
			if(IsDiscountInMoney)
			{
				DiscountMoney = value > Price * CurrentCount ? Price * CurrentCount : (value < 0 ? 0 : value);
				Discount = (100 * DiscountMoney) / (Price * CurrentCount);
			}
			else
			{
				Discount = value > 100 ? 100 : (value < 0 ? 0 : value);
				DiscountMoney = Price * CurrentCount * Discount / 100;
			}
		}

		private decimal GetPercentDiscount() => IsDiscountInMoney ? (100 * DiscountMoney) / (Price * CurrentCount) : Discount;

		public virtual void SetDiscountByStock(DiscountReason discountReasonForStockBottle, decimal discountPercent)
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

		public virtual decimal PriceWithoutVat => Math.Round(Price / ((ValueAddedTax ?? 0m) + 1) * (1 - Discount / 100), 2);
		public virtual decimal SumWithoutVat =>
			Math.Round(Price * CurrentCount - (IncludeNDS ?? 0 * (1 - Discount / 100)) - DiscountMoney, 2);

		public virtual decimal CurrentCount => ActualCount ?? Count;

		public virtual decimal Sum => Math.Round(Price * Count - DiscountMoney, 2);

		public virtual decimal ActualSum => Math.Round(Price * CurrentCount - DiscountMoney, 2);

		public virtual decimal OriginalSum => Math.Round(Price * Count - (OriginalDiscountMoney ?? 0), 2);

		public virtual bool CanEditAmount
		{
			get
			{
				bool result = true;

				if(RentType != OrderRentType.None)
				{
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

		public virtual bool CanEditPrice
		{
			get
			{
				if(PromoSet != null)
				{
					return false;
				}

				if(RentType != OrderRentType.None)
				{
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
			if(Nomenclature.Category == NomenclatureCategory.water)
			{
				var fixedPrice = _order.GetFixedPriceOrNull(Nomenclature, TotalCountInOrder);
				if(fixedPrice != null)
				{
					return fixedPrice.Price;
				}
			}
			return result;
		}

		public virtual decimal TotalCountInOrder =>
			Nomenclature.IsWater19L
			? Order.GetTotalWater19LCount(doNotCountWaterFromPromoSets: true)
			: Count;

		public virtual void RecalculatePrice()
		{
			var fixedPrice = Order.GetFixedPriceOrNull(Nomenclature, TotalCountInOrder);

			if(fixedPrice != null && CopiedFromUndelivery == null)
			{
				if(Price != fixedPrice.Price)
				{
					Price = fixedPrice.Price;
				}
				IsFixedPrice = true;
				return;
			}

			IsFixedPrice = false;

			if(IsUserPrice || PromoSet != null || Order.OrderStatus == OrderStatus.Closed || CopiedFromUndelivery != null)
				return;

			Price = GetPriceByTotalCount();
		}

		public virtual decimal GetPriceByTotalCount()
		{
			if(Nomenclature != null)
			{
				var curCount = Nomenclature.IsWater19L ? Order.GetTotalWater19LCount(doNotCountWaterFromPromoSets: true) : Count;
				var canApplyAlternativePrice = Order.HasPermissionsForAlternativePrice && Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount);

				if(Nomenclature.DependsOnNomenclature == null)
					return Nomenclature.GetPrice(curCount, canApplyAlternativePrice);
				if(Nomenclature.IsWater19L)
					return Nomenclature.DependsOnNomenclature.GetPrice(curCount, canApplyAlternativePrice);
			}
			return 0m;
		}

		public virtual CounterpartyMovementOperation UpdateCounterpartyOperation(IUnitOfWork uow)
		{
			if(!ActualCount.HasValue || ActualCount.Value == 0)
			{
				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Id > 0)
					uow.Delete(CounterpartyMovementOperation);

				CounterpartyMovementOperation = null;
				return null;
			}

			if(Nomenclature == null)
				throw new InvalidOperationException("Номенклатура не может быть null");

			if(CounterpartyMovementOperation == null)
			{
				CounterpartyMovementOperation = new CounterpartyMovementOperation
				{
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
			if(!NHibernateUtil.IsInitialized(Nomenclature))
			{
				NHibernateUtil.Initialize(Nomenclature);
			}

			if(!NHibernateUtil.IsInitialized(Order))
			{
				NHibernateUtil.Initialize(Order);
			}

			if(Order == null || Nomenclature == null)
			{
				return;
			}

			VAT vat = CanUseVAT() ? Nomenclature.VAT : VAT.No;

			switch(vat)
			{
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
			if(Order == null)
			{
				return;
			}
			if(!CanUseVAT())
			{
				IncludeNDS = null;
				return;
			}

			if(CanUseVAT() && ValueAddedTax.HasValue)
			{
				IncludeNDS = Math.Round(ActualSum * ValueAddedTax.Value / (1 + ValueAddedTax.Value), 2);
			}
		}

		private bool CanUseVAT()
		{
			if(!NHibernateUtil.IsInitialized(Order))
			{
				NHibernateUtil.Initialize(Order);
			}

			bool canUseVAT = true;
			if(Order.Contract?.Organization != null)
			{
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

	public enum OrderItemRentSubType
	{
		[Display(Name = "Нет аренды")]
		None,

		[Display(Name = "Услуга аренды")]
		RentServiceItem,

		[Display(Name = "Залог за аренду")]
		RentDepositItem
	}
}

