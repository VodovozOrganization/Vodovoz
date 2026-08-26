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
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Extensions;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки заказа",
		Nominative = "строка заказа")]
	[HistoryTrace]
	public class OrderItem :
		OrderItemEntity,
		IOrderItemWageCalculationSource,
		IProduct,
		IOrderSaleItem,
		IRecalculateRentCount,
		IPreserveDiscount
	{
		private Order _order;
		private Equipment _equipment;
		private CounterpartyMovementOperation _counterpartyMovementOperation;
		private PaidRentPackage _paidRentPackage;
		private FreeRentPackage _freeRentPackage;
		private OrderItem _copiedFromUndelivery;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;
		private PersonalDiscount _personalDiscount;
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
		
		/// <summary>
		/// Персональная скидка
		/// </summary>
		[Display(Name = "Персональная скидка")]
		public virtual PersonalDiscount PersonalDiscount
		{
			get => _personalDiscount;
			set => SetField(ref _personalDiscount, value);
		}

		[Display(Name = "Основания скидки на товар")]
		public virtual IObservableList<DiscountReason> DiscountReasons
		{
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}
		
		IEnumerable<DiscountReason> IDiscountReasons.DiscountReasons => DiscountReasons;

		[Display(Name = "Основания скидки на товар до отмены заказа")]
		public virtual IObservableList<DiscountReason> OriginalDiscountReasons
		{
			get => _originalDiscountReasons;
			set => SetField(ref _originalDiscountReasons, value);
		}

		#endregion

		#region IApplyDiscountReason implementation

		decimal IApplyDiscountReasonItem.CurrentRawPrice => CurrentRawPrice;

		public virtual IDiscountValue DiscountData => DiscountValue.Create(IsDiscountInMoney, Discount, DiscountMoney);

		IList<DiscountReason> IApplyDiscountReasonItem.DiscountReasons => DiscountReasons;
		
		public virtual void SetDiscount(IDiscountValue discountValue)
		{
			SetDiscountValuesBatch(discountValue);
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

		private decimal CurrentRawPrice => Price * CurrentCount;

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

				return NomenclatureEntity.GetCategoriesWithEditablePrice().Contains(Nomenclature.Category);
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

		/*public virtual bool IsDiscountValueCanBeAdded(bool isDiscountInMoney, decimal discount)
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
		}*/

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

		internal static OrderItem CreateNewDailyRentServiceItem(IOrderSaleHandler saleHandler, Order order, PaidRentPackage paidRentPackage)
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

			newItem.UpdatePriceWithRecalculate((SaleItemPriceType.General, paidRentPackage.PriceDaily), saleHandler);

			return newItem;
		}

		internal static OrderItem CreateNewDailyRentDepositItem(IOrderSaleHandler saleHandler, Order order, PaidRentPackage paidRentPackage)
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

			newItem.UpdatePriceWithRecalculate((SaleItemPriceType.General, paidRentPackage.Deposit), saleHandler);

			return newItem;
		}

		internal static OrderItem CreateNewNonFreeRentServiceItem(IOrderSaleHandler saleHandler, Order order, PaidRentPackage paidRentPackage)
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

			newItem.UpdatePriceWithRecalculate((SaleItemPriceType.General, paidRentPackage.PriceMonthly), saleHandler);

			return newItem;
		}

		internal static OrderItem CreateNewNonFreeRentDepositItem(IOrderSaleHandler saleHandler, Order order, PaidRentPackage paidRentPackage)
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

			newItem.UpdatePriceWithRecalculate((SaleItemPriceType.General, paidRentPackage.Deposit), saleHandler);

			return newItem;
		}

		internal static OrderItem CreateNewFreeRentDepositItem(IOrderSaleHandler saleHandler, Order order, FreeRentPackage freeRentPackage)
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

			newItem.UpdatePriceWithRecalculate((SaleItemPriceType.General, freeRentPackage.Deposit), saleHandler);

			return newItem;
		}

		internal static OrderItem CreateForSale(
			IOrderSaleHandler saleHandler,
			Order order,
			NewOrderSaleItem newOrderSaleItem)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = newOrderSaleItem.Count,
				Equipment = newOrderSaleItem.Equipment,
				Nomenclature = newOrderSaleItem.Nomenclature,
				GiftItem = newOrderSaleItem.GiftItem
			};

			newItem.UpdatePriceWithRecalculate(newOrderSaleItem.PriceData, saleHandler);

			return newItem;
		}

		internal static OrderItem CreateForSaleWithDiscount(
			IOrderSaleHandler saleHandler,
			Order order,
			NewOrderSaleItem newOrderSaleItem
			)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = newOrderSaleItem.Count,
				Equipment = null,
				Nomenclature = newOrderSaleItem.Nomenclature,
				IsDiscountInMoney = newOrderSaleItem.IsDiscountInMoney,
				PromoSet = newOrderSaleItem.PromoSet,
				GiftItem = newOrderSaleItem.GiftItem
			};

			newItem.UpdatePriceWithRecalculate(newOrderSaleItem.PriceData, saleHandler);

			if(newOrderSaleItem.DiscountReasons != null && newOrderSaleItem.DiscountReasons.Any())
			{
				foreach(var reason in newOrderSaleItem.DiscountReasons)
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

			newItem.CalculateAndSetDiscount(newOrderSaleItem.Discount);

			return newItem;
		}

		internal static OrderItem CreateDeliveryOrderItem(
			IOrderSaleHandler saleHandler,
			Order order,
			Nomenclature nomenclature,
			decimal price)
		{
			var newItem = new OrderItem
			{
				Order = order,
				Count = 1,
				Nomenclature = nomenclature
			};

			newItem.UpdatePriceWithRecalculate((SaleItemPriceType.User, price), saleHandler);

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

		#region implementations

		#region ICount implementaiton

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
		
		#region IRecalculateRentCount implementaiton
		
		int IRecalculateRentCount.RentCount
		{
			get => RentCount;
			set
			{
				if(RentCount == value)
				{
					return;
				}
				
				RentCount = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region IPreserveDiscount implementation

		IList<DiscountReason> IPreserveDiscount.OriginalDiscountReasons => OriginalDiscountReasons;

		#endregion

		#region IOrderSaleItem implementation

		decimal? IOrderSaleItem.ActualCount
		{
			get => ActualCount;
			set
			{
				if(ActualCount == value)
				{
					return;
				}
				
				ActualCount = value;
				OnPropertyChanged();
			}
		}

		bool IOrderSaleItem.CopiedFromUndelivery => CopiedFromUndelivery != null;

		#endregion

		#endregion
	}
}
