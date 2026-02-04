using Autofac;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Extensions;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Controllers;
using static VodovozBusiness.Services.Orders.CreateOrderRequest;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки заказа",
		Nominative = "строка заказа")]
	[HistoryTrace]
	public class OrderItem : OrderItemEntity, IOrderItemWageCalculationSource, IDiscount, IProduct
	{
		private Order _order;
		private Equipment _equipment;
		private DiscountReason _originalDiscountReason;
		private CounterpartyMovementOperation _counterpartyMovementOperation;
		private PaidRentPackage _paidRentPackage;
		private FreeRentPackage _freeRentPackage;
		private OrderItem _copiedFromUndelivery;
		private DiscountReason _discountReason;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;
		private INomenclatureSettings _nomenclatureSettings => ScopeProvider.Scope.Resolve<INomenclatureSettings>();

		protected OrderItem()
		{
		}

		#region Свойства

		[Display(Name = "Заказ")]
		public virtual new Order Order
		{
			get => _order;
			protected set => SetField(ref _order, value);
		}

		[Display(Name = "Оборудование")]
		public virtual Equipment Equipment
		{
			get => _equipment;
			set => SetField(ref _equipment, value);
		}

		[Display(Name = "Основание скидки на товар до отмены заказа")]
		public virtual DiscountReason OriginalDiscountReason
		{
			get => _originalDiscountReason;
			set => SetField(ref _originalDiscountReason, value);
		}

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation
		{
			get => _counterpartyMovementOperation;
			set => SetField(ref _counterpartyMovementOperation, value);
		}

		#region Аренда

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

		#endregion Аренда

		public virtual new OrderItem CopiedFromUndelivery
		{
			get => _copiedFromUndelivery;
			set => SetField(ref _copiedFromUndelivery, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature
		{
			get => _nomenclature;
			protected set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Добавлено из промонабора")]
		public virtual PromotionalSet PromoSet
		{
			get => _promoSet;
			set => SetField(ref _promoSet, value);
		}

		[Display(Name = "Основание скидки на товар")]
		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}

		#endregion

		#region Вычисляемые

		public virtual bool CanShowReturnedCount =>
			Order.OrderStatus >= OrderStatus.OnTheWay && ReturnedCount > 0
			&& Nomenclature.GetCategoriesForShipment().Contains(Nomenclature.Category);

		public virtual bool IsDepositCategory =>
			Nomenclature.Category == NomenclatureCategory.deposit;

		public virtual decimal ManualChangingDiscount
		{
			get => GetDiscount;
			protected set
			{
				CalculateAndSetDiscount(value);
				if(DiscountByStock != 0)
				{
					DiscountByStock = 0;
					DiscountReason = null;
				}
			}
		}

		public virtual decimal GetDiscount => IsDiscountInMoney ? DiscountMoney : Discount;

		public virtual void UpdateRentCount(int rentCount)
		{
			if(RentCount == rentCount)
			{
				return;
			}

			RentCount = rentCount;
			Order?.UpdateRentsCount();
		}

		public virtual void SetRentEquipmentCount(int equipmentCount)
		{
			RentEquipmentCount = equipmentCount;
			switch(OrderItemRentSubType)
			{
				case OrderItemRentSubType.RentServiceItem:
					SetCount(RentCount * RentEquipmentCount);
					break;
				case OrderItemRentSubType.RentDepositItem:
					SetCount(RentEquipmentCount);
					break;
			}
		}

		private void RecalculateDiscount()
		{
			if(!CheckInitializedProperties())
			{
				return;
			}

			if(CurrentCount == 0)
			{
				if(Order.IsUndeliveredStatus)
				{
					RemoveAndPreserveDiscount();
				}
				else
				{
					ClearDiscount();
				}
			}
			else
			{
				var discount = Discount == 0 && DiscountReason != null
					? DiscountReason.Value
					: IsDiscountInMoney
						? DiscountMoney
						: Discount;

				CalculateAndSetDiscount(discount);
			}
		}

		private bool CheckInitializedProperties()
		{
			if(!NHibernateUtil.IsPropertyInitialized(this, nameof(DiscountMoney))
			   || !NHibernateUtil.IsPropertyInitialized(this, nameof(Discount))
			   || !NHibernateUtil.IsPropertyInitialized(this, nameof(Price))
			   || (Order == null || !NHibernateUtil.IsInitialized(Order.OrderItems)))
			{
				return false;
			}

			return true;
		}

		private void RecalculateDiscountWithPreserveOrRestoreDiscount()
		{
			if(!CheckInitializedProperties())
			{
				return;
			}
			
			if(CurrentCount == 0)
			{
				RemoveAndPreserveDiscount();
			}
			else
			{
				RestoreOriginalDiscount();
			}
		}

		public virtual void RemoveAndPreserveDiscount()
		{
			if(DiscountMoney > 0)
			{
				OriginalDiscountMoney = DiscountMoney;
				OriginalDiscountReason = DiscountReason;
				OriginalDiscount = Discount;
			}
			DiscountMoney = 0;
			Discount = 0;
			DiscountReason = null;

			RecalculateVAT();
		}

		public virtual void RemoveDiscount()
		{
			if(DiscountReason == null)
			{
				return;
			}

			ClearDiscount();
			RecalculateVAT();
		}

		public virtual void SetNomenclature(Nomenclature nomenclature)
		{
			Nomenclature = nomenclature;
			CalculateVATType();
		}

		private void ClearDiscount()
		{
			DiscountReason = null;
			IsDiscountInMoney = false;
			DiscountMoney = default;
			Discount = default;
		}

		private void CalculateAndSetDiscount(decimal value)
		{
			if(value == 0)
			{
				DiscountReason = null;
			}

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

			RecalculateVAT();
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

			RecalculateVAT();
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

				if(Nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId)
				{
					return false;
				}

				return Nomenclature.GetCategoriesWithEditablePrice().Contains(Nomenclature.Category);
			}
		}

		public virtual string NomenclatureString => Nomenclature != null ? Nomenclature.Name : string.Empty;

		public virtual string Title => $"[{Order.Title}] {Nomenclature.Name} - {Count}*{Price}={Sum}";

		public virtual decimal TotalCountInOrder =>
			Nomenclature.IsWater19L
			? Order.GetTotalWater19LCount(true, true)
			: Count;

		public virtual bool IsTrueMarkCodesMustBeAdded =>
			Nomenclature?.IsAccountableInTrueMark == true
			&& Count > 0;

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
			{
				return result;
			}

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

		public virtual void RecalculatePrice()
		{
			if(IsUserPrice || PromoSet != null || Order.OrderStatus == OrderStatus.Closed || CopiedFromUndelivery != null)
			{
				return;
			}

			var fixedPrice = Order.GetFixedPriceOrNull(Nomenclature, TotalCountInOrder);

			if(fixedPrice != null && CopiedFromUndelivery == null)
			{
				IsFixedPrice = true;
				if(Price != fixedPrice.Price)
				{
					SetPrice(fixedPrice.Price);
				}
				return;
			}

			IsFixedPrice = false;

			SetPrice(GetPriceByTotalCount());
		}

		public virtual decimal GetPriceByTotalCount()
		{
			if(Nomenclature != null)
			{
				var curCount = Nomenclature.IsWater19L ? Order.GetTotalWater19LCount(true, true) : Count;
				var canApplyAlternativePrice = Order.HasPermissionsForAlternativePrice && Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount);

				if(Nomenclature.DependsOnNomenclature == null)
				{
					return Nomenclature.GetPrice(curCount, canApplyAlternativePrice);
				}

				if(Nomenclature.IsWater19L)
				{
					return Nomenclature.DependsOnNomenclature.GetPrice(curCount, canApplyAlternativePrice);
				}
			}
			return 0m;
		}

		public virtual CounterpartyMovementOperation UpdateCounterpartyOperation(IUnitOfWork uow)
		{
			if(!ActualCount.HasValue || ActualCount.Value == 0)
			{
				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Id > 0)
				{
					uow.Delete(CounterpartyMovementOperation);
				}

				CounterpartyMovementOperation = null;
				return null;
			}

			if(Nomenclature == null)
			{
				throw new InvalidOperationException("Номенклатура не может быть null");
			}

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
			else
			{
				CounterpartyMovementOperation.Amount = ActualCount.Value;
				CounterpartyMovementOperation.IncomingCounterparty = Order.Client;
				CounterpartyMovementOperation.IncomingDeliveryPoint = Order.DeliveryPoint;
			}

			return CounterpartyMovementOperation;
		}
		
		public virtual bool HasZeroCountOrSum() => Count <= 0 || Sum == default;

		public virtual bool IsTrueMarkCodesMustBeAddedInWarehouse(ICounterpartyEdoAccountController edoAccountController)
		{
			return IsTrueMarkCodesMustBeAdded
				&& (Order.IsNeedIndividualSetOnLoad(edoAccountController) || Order.IsNeedIndividualSetOnLoadForTender);
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

			var organization = Order.Contract?.Organization;
			
			var vatRateVersion =  organization != null && organization.IsUsnMode 
				? Order.Contract.Organization.GetActualVatRateVersion(Order.DeliveryDate)
				: Nomenclature.GetActualVatRateVersion(Order.DeliveryDate);
			
			if(vatRateVersion == null)
			{
				throw new InvalidOperationException($"У товара #{Nomenclature.Id} отсутствует версия НДС на дату счета заказа #{Order.BillDate}");
			}
			
			ValueAddedTax =  CanUseVAT() ? vatRateVersion.VatRate.VatNumericValue : 0;
			
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
				canUseVAT = Order.Contract.Organization.IsUsnMode 
					? Order.Contract.Organization.GetActualVatRateVersion(Order.DeliveryDate)?.VatRate.VatNumericValue != 0
					: Nomenclature.GetActualVatRateVersion(Order.DeliveryDate)?.VatRate.VatNumericValue != 0;
			}

			return canUseVAT;
		}

		#endregion

		/// <summary>
		/// Устанавливает ActualCount из Count
		/// </summary>
		protected internal virtual void PreserveActualCount(bool ignoreHasValue = false)
		{
			if(ignoreHasValue || !ActualCount.HasValue)
			{
				ActualCount = Count;

				RecalculateDiscount();
				RecalculateVAT();
			}
		}

		public virtual void SetActualCount(decimal? newValue)
		{
			ActualCount = newValue;

			RecalculateDiscount();
			RecalculateVAT();
		}
		
		public virtual void SetActualCountWithPreserveOrRestoreDiscount(decimal? newValue)
		{
			ActualCount = newValue;
			RecalculateDiscountWithPreserveOrRestoreDiscount();
		}

		public virtual void SetActualCountZero()
		{
			SetActualCount(0m);
		}

		public virtual void SetPrice(decimal price)
		{
			//Если цена не отличается от той которая должна быть по прайсам в 
			//номенклатуре, то цена не изменена пользователем и сможет расчитываться автоматически
			IsUserPrice = (price != GetPriceByTotalCount() && price != 0 && !IsFixedPrice) || CopiedFromUndelivery != null;

			price = decimal.Round(price, 2);

			if(Price != price)
			{
				Price = price;

				RecalculateDiscount();
				RecalculateVAT();
			}
		}

		protected internal virtual void SetCount(decimal count)
		{
			if(Nomenclature?.Unit?.Digits == 0 && count % 1 != 0)
			{
				count = Math.Truncate(count);
			}

			if(Count != count)
			{
				Count = count < 0 ? 0 : count;
				Order?.RecalculateItemsPrice();
				RecalculateDiscount();
				RecalculateVAT();
				Order?.UpdateRentsCount();
			}
		}

		protected internal virtual void RestoreOriginalDiscountFromRestoreOrder()
		{
			TryRestoreOriginalDiscount();
			ActualCount = null;

			RecalculateDiscount();
			RecalculateVAT();
		}
		
		private void RestoreOriginalDiscount()
		{
			TryRestoreOriginalDiscount();
			CalculateAndSetDiscount(IsDiscountInMoney ? DiscountMoney : Discount);
		}

		private void TryRestoreOriginalDiscount()
		{
			if(OriginalDiscountMoney.HasValue || OriginalDiscount.HasValue)
			{
				DiscountMoney = OriginalDiscountMoney ?? 0;
				DiscountReason = OriginalDiscountReason;
				Discount = OriginalDiscount ?? 0;
				OriginalDiscountMoney = null;
				OriginalDiscountReason = null;
				OriginalDiscount = null;
			}
		}

		public virtual void SetDiscount(decimal discount)
		{
			if(discount != Discount && discount == 0)
			{
				DiscountReason = null;
			}

			CalculateAndSetDiscount(discount);
			RecalculateVAT();
		}

		public virtual void SetIsDiscountInMoney(bool isDiscountInMoney)
		{
			IsDiscountInMoney = isDiscountInMoney;
			RecalculateVAT();
		}

		public virtual void SetManualChangingDiscount(decimal manualChangingDiscount)
		{
			ManualChangingDiscount = manualChangingDiscount;
		}

		public virtual void SetDiscount(bool isDiscountInMoney, decimal discount, DiscountReason discountReason)
		{
			IsDiscountInMoney = isDiscountInMoney;
			CalculateAndSetDiscount(discount);
			DiscountReason = discountReason;
			RecalculateVAT();
		}

		protected internal virtual void SetDiscount(bool isDiscountInMoney, decimal discount, decimal discountMoney, DiscountReason discountReason)
		{
			IsDiscountInMoney = isDiscountInMoney;
			Discount = discount;
			DiscountMoney = discountMoney;
			DiscountReason = discountReason;
			RecalculateVAT();
		}

		protected internal virtual void RecalculateDiscountAndVat()
		{
			RecalculateDiscount();
			CalculateVATType();
		}

		internal static OrderItem CreateNewDailyRentServiceItem(Order order, PaidRentPackage paidRentPackage)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = 1,
				RentCount = 1,
				RentType = OrderRentType.DailyRent,
				OrderItemRentSubType = OrderItemRentSubType.RentServiceItem,
				PaidRentPackage = paidRentPackage,
				Nomenclature = paidRentPackage.RentServiceDaily
			};

			newItem.UpdatePriceWithRecalculate(paidRentPackage.PriceDaily);

			return newItem;
		}

		internal static OrderItem CreateNewDailyRentDepositItem(Order order, PaidRentPackage paidRentPackage)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = 1,
				RentType = OrderRentType.DailyRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				PaidRentPackage = paidRentPackage,
				Nomenclature = paidRentPackage.DepositService
			};

			newItem.UpdatePriceWithRecalculate(paidRentPackage.Deposit);

			return newItem;
		}

		internal static OrderItem CreateNewNonFreeRentServiceItem(Order order, PaidRentPackage paidRentPackage)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = 1,
				RentCount = 1,
				RentType = OrderRentType.NonFreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentServiceItem,
				PaidRentPackage = paidRentPackage,
				Nomenclature = paidRentPackage.RentServiceMonthly
			};

			newItem.UpdatePriceWithRecalculate(paidRentPackage.PriceMonthly);

			return newItem;
		}

		internal static OrderItem CreateNewNonFreeRentDepositItem(Order order, PaidRentPackage paidRentPackage)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = 1,
				RentType = OrderRentType.NonFreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				PaidRentPackage = paidRentPackage,
				Nomenclature = paidRentPackage.DepositService
			};

			newItem.UpdatePriceWithRecalculate(paidRentPackage.Deposit);

			return newItem;
		}

		internal static OrderItem CreateNewFreeRentDepositItem(Order order, FreeRentPackage freeRentPackage)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = 1,
				RentType = OrderRentType.FreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				FreeRentPackage = freeRentPackage,
				Nomenclature = freeRentPackage.DepositService
			};

			newItem.UpdatePriceWithRecalculate(freeRentPackage.Deposit);

			return newItem;
		}

		internal static OrderItem CreateForSale(Order order, Nomenclature nomenclature, decimal count, decimal price) =>
			CreateForSale(order, nomenclature, null, count, price);

		internal static OrderItem CreateForSale(Order order, Nomenclature nomenclature, Equipment equipment, decimal count, decimal price)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = count,
				Equipment = equipment,
				Nomenclature = nomenclature
			};

			newItem.UpdatePriceWithRecalculate(price);

			return newItem;
		}

		internal static OrderItem CreateForSaleWithDiscount(
			Order order,
			Nomenclature nomenclature,
			decimal count,
			decimal price,
			bool isDiscountInMoney,
			decimal discount,
			DiscountReason discountReason,
			PromotionalSet promotionalSet)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = count,
				Equipment = null,
				Nomenclature = nomenclature,
				IsDiscountInMoney = isDiscountInMoney,
				DiscountReason = discountReason,
				PromoSet = promotionalSet
			};

			newItem.UpdatePriceWithRecalculate(price);
			newItem.CalculateAndSetDiscount(discount);

			return newItem;
		}

		internal static OrderItem CreateDeliveryOrderItem(Order order, Nomenclature nomenclature, decimal price)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = 1,
				Nomenclature = nomenclature
			};

			newItem.UpdatePriceWithRecalculate(price);

			return newItem;
		}

		public static OrderItem CreateEmptyWithId(int id)
		{
			return new OrderItem { Id = id };
		}
	}
}
