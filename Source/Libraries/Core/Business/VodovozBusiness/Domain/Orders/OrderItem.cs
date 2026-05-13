using Autofac;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Extensions;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Controllers;

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
		private CounterpartyMovementOperation _counterpartyMovementOperation;
		private PaidRentPackage _paidRentPackage;
		private FreeRentPackage _freeRentPackage;
		private OrderItem _copiedFromUndelivery;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;
		private IObservableList<DiscountReason> _discountReasons = new ObservableList<DiscountReason>();
		private IObservableList<DiscountReason> _originalDiscountReasons = new ObservableList<DiscountReason>();
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

		[Display(Name = "Основания скидки на товар")]
		public virtual IObservableList<DiscountReason> DiscountReasons
		{
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}

		[Display(Name = "Основания скидки на товар до отмены заказа")]
		public virtual IObservableList<DiscountReason> OriginalDiscountReasons
		{
			get => _originalDiscountReasons;
			set => SetField(ref _originalDiscountReasons, value);
		}

		#endregion

		#region Вычисляемые

		public virtual bool CanShowReturnedCount =>
			Order.OrderStatus >= OrderStatus.OnTheWay && ReturnedCount > 0
			&& Nomenclature.GetCategoriesForShipment().Contains(Nomenclature.Category);

		public virtual decimal ManualChangingDiscount
		{
			get => GetDiscount;
			protected set
			{
				CalculateAndSetDiscount(value);
				if(DiscountByStock != 0)
				{
					DiscountByStock = 0;
					DiscountReasons.Clear();
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
				var discount = IsDiscountInMoney
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

		private void RecalculateTotalDiscountFromReasons()
		{
			var currentPrice = CurrentRawPrice;
			var totalDiscountMoney = CalculateTotalDiscountInMoneyFromAddedReasons();

			var discountMoney =
				DiscountReasons.All(x => x.ValueType == DiscountUnits.money)
				? DiscountReasons.Sum(x => x.Value)
				: totalDiscountMoney;

			var discount =
				DiscountReasons.All(x => x.ValueType == DiscountUnits.percent)
				? DiscountReasons.Sum(x => x.Value)
				: currentPrice > 0 ? (100 * discountMoney) / currentPrice : 0;

			var isDiscountInMoney = DiscountReasons.Any(x => x.ValueType == DiscountUnits.money);

			if(discountMoney > currentPrice)
			{
				discountMoney = currentPrice;
			}

			if(discount > 100)
			{
				discount = 100;
			}

			SetDiscountValuesBatch(discountMoney, discount, isDiscountInMoney);

			RecalculateVAT();
		}

		private decimal CurrentRawPrice => Price * CurrentCount;

		private decimal CalculateTotalDiscountInMoneyFromAddedReasons()
		{
			decimal currentPrice = CurrentRawPrice;

			decimal totalPercentDiscount = 0;
			decimal totalMoneyDiscount = 0;

			foreach(var reason in DiscountReasons)
			{
				if(reason.ValueType is DiscountUnits.money)
				{
					totalMoneyDiscount += reason.Value;
				}
				else
				{
					totalPercentDiscount += reason.Value;
				}
			}

			decimal discountFromPercent = currentPrice * (totalPercentDiscount / 100);
			decimal totalDiscountMoney = discountFromPercent + totalMoneyDiscount;

			return totalDiscountMoney;
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

		/// <summary>
		/// Удаляет текущие скидки и сохраняет их в <see cref="OriginalDiscountReasons"/>.
		/// Восстановить скидку можно методом <see cref="RestoreOriginalDiscount"/>.
		/// </summary>
		public virtual void RemoveAndPreserveDiscount()
		{
			if(DiscountMoney > 0)
			{
				OriginalDiscountMoney = DiscountMoney;
				OriginalDiscount = Discount;

				OriginalDiscountReasons.Clear();
				foreach(var reason in DiscountReasons)
				{
					OriginalDiscountReasons.Add(reason);
				}
			}
			DiscountMoney = 0;
			Discount = 0;
			DiscountReasons.Clear();

			RecalculateVAT();
		}

		/// <summary>
		/// Удаляет все скидки
		/// </summary>
		public virtual void ClearDiscounts()
		{
			if(!DiscountReasons.Any())
			{
				return;
			}

			ClearDiscount();
			RecalculateVAT();
		}

		/// <summary>
		/// Удаляет скидки
		/// </summary>
		public virtual void RemoveDiscount(int discountReasonId)
		{
			if(!DiscountReasons.Any())
			{
				return;
			}

			var reasonsToRemove = DiscountReasons.Where(r => r.Id == discountReasonId).ToList();

			foreach(var reason in reasonsToRemove)
			{
				DiscountReasons.Remove(reason);
			}

			RecalculateTotalDiscountFromReasons();
		}

		public virtual void SetNomenclature(Nomenclature nomenclature)
		{
			Nomenclature = nomenclature;
			CalculateVATType();
		}

		private void ClearDiscount()
		{
			DiscountReasons.Clear();
			IsDiscountInMoney = false;
			DiscountMoney = 0;
			Discount = 0;
		}

		private void CalculateAndSetDiscount(decimal value)
		{
			if(value == 0)
			{
				DiscountReasons.Clear();
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
				DiscountReasons.Clear();
			}
			else if((!DiscountReasons.Any() && PromoSet == null) || (!DiscountReasons.Any() && PromoSet != null && existingPercent == 0))
			{
				DiscountReasons.Add(discountReasonForStockBottle);
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

		public override void CalculateVATType()
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

			var vatRateVersion = Nomenclature.GetEffectiveVatRateVersion(organization, Order.DeliveryDate);
			
			if(vatRateVersion == null)
			{
				throw new InvalidOperationException($"У товара #{Nomenclature.Id} отсутствует версия НДС на дату доставки #{Order.DeliveryDate}");
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
				canUseVAT = Nomenclature.GetEffectiveVatRateVersion(Order.Contract.Organization, Order.DeliveryDate)?.VatRate.VatNumericValue != 0;
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
				Discount = OriginalDiscount ?? 0;

				DiscountReasons.Clear();
				foreach(var reason in OriginalDiscountReasons)
				{
					DiscountReasons.Add(reason);
				}

				OriginalDiscountMoney = null;
				OriginalDiscount = null;
				OriginalDiscountReasons.Clear();
			}
		}

		/// <summary>
		/// Устанавливает скидку в процентах или деньгах.
		/// При значении 0 очищает все скидки.
		/// </summary>
		/// <param name="discount">Значение скидки (проценты 0-100 или деньги 0-цена товара)</param>
		public virtual void SetDiscount(decimal discount)
		{
			if(discount != Discount && discount == 0)
			{
				DiscountReasons.Clear();
			}

			CalculateAndSetDiscount(discount);
			RecalculateVAT();
		}

		/// <summary>
		/// Устанавливает тип скидки (проценты или деньги).
		/// </summary>
		/// <param name="isDiscountInMoney">true - скидка в деньгах, false - в процентах</param>
		public virtual void SetIsDiscountInMoney(bool isDiscountInMoney)
		{
			IsDiscountInMoney = isDiscountInMoney;
			RecalculateVAT();
		}

		/// <summary>
		/// Устанавливает ручное изменение скидки.
		/// Используется при ручном редактировании скидки пользователем.
		/// </summary>
		/// <param name="manualChangingDiscount">Новое значение скидки</param>
		public virtual void SetManualChangingDiscount(decimal manualChangingDiscount)
		{
			ManualChangingDiscount = manualChangingDiscount;
		}

		/// <summary>
		/// Устанавливает скидку с указанием типа и основания
		/// </summary>
		/// <param name="isDiscountInMoney">true - скидка в деньгах, false - в процентах</param>
		/// <param name="discount">Значение скидки</param>
		/// <param name="discountReason">Основание скидки</param>
		public virtual void AddDiscount(bool isDiscountInMoney, decimal discount, DiscountReason discountReason)
		{
			if(discountReason != null && !IsDiscountReasonAdded(discountReason))
			{
				DiscountReasons.Add(discountReason);
			}

			RecalculateTotalDiscountFromReasons();
		}

		public virtual bool IsDiscountValueCanBeAdded(bool isDiscountInMoney, decimal discount)
		{
			var isCalculateInPercent =
				DiscountReasons.All(x => x.ValueType == DiscountUnits.percent) && !isDiscountInMoney;

			if(isCalculateInPercent)
			{
				var totalPercentDiscount = DiscountReasons.Sum(x => x.Value) + discount;
				return totalPercentDiscount <= 100;
			}

			var alreadyAddedDiscount = CalculateTotalDiscountInMoneyFromAddedReasons();
			var discountMoneyToAdd = isDiscountInMoney ? discount : CurrentRawPrice * discount / 100;

			return discountMoneyToAdd + alreadyAddedDiscount <= CurrentRawPrice;
		}

		public virtual bool IsDiscountReasonAdded(DiscountReason discountReason)
		{
			if(discountReason is null)
			{
				throw new ArgumentNullException(nameof(discountReason));
			}
			
			return DiscountReasons.Any(x => x.Id == discountReason.Id);
		}

		protected internal virtual void SetDiscount(bool isDiscountInMoney, decimal discount, decimal discountMoney, IList<DiscountReason> discountReasons)
		{
			IsDiscountInMoney = isDiscountInMoney;
			Discount = discount;
			DiscountMoney = discountMoney;

			DiscountReasons.Clear();
			foreach(var reason in discountReasons)
			{
				if(reason != null && !DiscountReasons.Contains(reason))
				{
					DiscountReasons.Add(reason);
				}
			}

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

		internal static OrderItem CreateForSale(Order order, Nomenclature nomenclature, decimal count, decimal price, bool isGift = false) =>
			CreateForSale(order, nomenclature, null, count, price, isGift);

		internal static OrderItem CreateForSale(
			Order order,
			Nomenclature nomenclature,
			Equipment equipment,
			decimal count,
			decimal price,
			bool isGift = false)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = count,
				Equipment = equipment,
				Nomenclature = nomenclature,
				IsGift = isGift
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
			IEnumerable<DiscountReason> discountReasons,
			PromotionalSet promotionalSet,
			bool isGift = false)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = count,
				Equipment = null,
				Nomenclature = nomenclature,
				IsDiscountInMoney = isDiscountInMoney,
				PromoSet = promotionalSet,
				IsGift = isGift
			};

			newItem.UpdatePriceWithRecalculate(price);

			if(discountReasons != null && discountReasons.Any())
			{
				foreach(var reason in discountReasons)
				{
					if(reason is null)
					{
						continue;
					}

					if(newItem.DiscountReasons.Any(x => x.Id == reason.Id))
					{
						continue;
					}

					newItem.DiscountReasons.Add(reason);
				}
			}

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

		/// <summary>
		/// Наименования оснований скидки через запятую
		/// </summary>
		public virtual string DiscountReasonsNames =>
			string.Join(", ", DiscountReasons.Select(x => x.Name));

		/// <summary>
		/// Наименования исходных оснований скидки до отмены заказа через запятую
		/// </summary>
		public virtual string OriginalDiscountReasonsNames =>
			string.Join(", ", OriginalDiscountReasons.Select(x => x.Name));
	}
}
