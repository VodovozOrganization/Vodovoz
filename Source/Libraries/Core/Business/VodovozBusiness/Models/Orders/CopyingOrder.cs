using Gamma.Utilities;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Models.Orders
{
	public class CopyingOrder
	{
		private readonly IUnitOfWork _uow;
		private readonly Order _copiedOrder;
		private readonly Order _resultOrder;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IFlyerRepository _flyerRepository;
		private readonly IOrderContractUpdater _contractUpdater;
		private readonly int _paidDeliveryNomenclatureId;
		private readonly IList<int> _flyersNomenclaturesIds;
		private readonly int _fastDeliveryNomenclatureId;
		private readonly int _masterCallNomenclatureId;
		private bool _needCopyStockBottleDiscount;

		internal CopyingOrder(
			IUnitOfWork uow,
			Order copiedOrder,
			Order resultOrder,
			INomenclatureSettings nomenclatureSettings,
			IFlyerRepository flyerRepository,
			IOrderContractUpdater contractUpdater)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_copiedOrder = copiedOrder ?? throw new ArgumentNullException(nameof(copiedOrder));
			_resultOrder = resultOrder ?? throw new ArgumentNullException(nameof(resultOrder));
			if(resultOrder.Id > 0)
			{
				throw new ArgumentException(
					$"Заказ, в который переносятся данные из копируемого заказа, должен быть новым. (Свойство {nameof(resultOrder.Id)} должно быть равно 0)");
			}

			_nomenclatureSettings =
				nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));

			_paidDeliveryNomenclatureId = _nomenclatureSettings.PaidDeliveryNomenclatureId;
			_fastDeliveryNomenclatureId = _nomenclatureSettings.FastDeliveryNomenclatureId;
			_masterCallNomenclatureId = _nomenclatureSettings.MasterCallNomenclatureId;
			_flyersNomenclaturesIds = _flyerRepository.GetAllFlyersNomenclaturesIds(_uow);
		}

		public Order GetCopiedOrder => _copiedOrder;

		public IEnumerable<OrderItem> OrderItemsFromCopiedOrderHavingDiscountsWithoutPromosets =>
			GetCopiedOrder.OrderItems
			.Where(x => x.PromoSet == null)
			.Where(x => x.Discount > 0 || x.OriginalDiscount > 0 || x.DiscountMoney > 0 || x.OriginalDiscountMoney > 0);

		/// <summary>
		/// Копирование основных полей заказа
		/// </summary>
		public CopyingOrder CopyFields(params Expression<Func<Order, object>>[] onlyFields)
		{
			if(onlyFields.Any())
			{
				foreach(var propertyExpression in onlyFields)
				{
					CopyExpressionField(propertyExpression);
				}
			}
			else
			{
				_resultOrder.UpdateClient(_copiedOrder.Client, _contractUpdater, out var message);
				_resultOrder.SelfDelivery = _copiedOrder.SelfDelivery;
				_resultOrder.UpdateDeliveryPoint(_copiedOrder.DeliveryPoint, _contractUpdater);
				_resultOrder.UpdatePaymentType(_copiedOrder.PaymentType, _contractUpdater);
				_resultOrder.Author = _copiedOrder.Author;
				_resultOrder.Comment = _copiedOrder.Comment;
				_resultOrder.CommentLogist = _copiedOrder.CommentLogist;
				_resultOrder.TareNonReturnReason = _copiedOrder.TareNonReturnReason;
				_resultOrder.BillDate = _copiedOrder.BillDate;
				_resultOrder.BottlesReturn = _copiedOrder.BottlesReturn;
				_resultOrder.CollectBottles = _copiedOrder.CollectBottles;
				_resultOrder.CommentManager = _copiedOrder.CommentManager;
				_resultOrder.DocumentType = _copiedOrder.DocumentType;
				_resultOrder.InformationOnTara = _copiedOrder.InformationOnTara;
				_resultOrder.ReturnedTare = _copiedOrder.ReturnedTare;
				_resultOrder.SignatureType = _copiedOrder.SignatureType;
				_resultOrder.Trifle = _copiedOrder.Trifle;
				_resultOrder.OrderAddressType = _copiedOrder.OrderAddressType;
				_resultOrder.ReturnTareReasonCategory = _copiedOrder.ReturnTareReasonCategory;
				_resultOrder.ReturnTareReason = _copiedOrder.ReturnTareReason;
				_resultOrder.ContactPhone = _copiedOrder.ContactPhone;
				_resultOrder.LogisticsRequirements = _copiedOrder.LogisticsRequirements;
				_resultOrder.IsSecondOrder = _copiedOrder.IsSecondOrder;
			}

			return this;
		}
		
		/// <summary>
		/// Копирование полей, связанных с акцией бутыль
		/// </summary>
		public CopyingOrder CopyStockBottle()
		{
			_resultOrder.IsBottleStock = _copiedOrder.IsBottleStock;
			_resultOrder.BottlesByStockCount = _copiedOrder.BottlesByStockCount;
			_needCopyStockBottleDiscount = true;

			return this;
		}

		/// <summary>
		/// Копирование дополнительных полей заказа
		/// </summary>
		/// <param name="propertyExpressions">Селектор свойств заказа. Необходимо выбирать только свойства</param>
		public CopyingOrder CopyAdditionalFields(params Expression<Func<Order, object>>[] propertyExpressions)
		{
			foreach(var propertyExpression in propertyExpressions)
			{
				CopyExpressionField(propertyExpression);
			}

			return this;
		}

		private void CopyExpressionField(Expression<Func<Order, object>> propertyExpression)
		{
			var propertyInfo = PropertyUtil.GetPropertyInfo(propertyExpression);
			if(propertyInfo == null)
			{
				throw new ArgumentException($"Должно быть выбрано только свойство.");
			}

			switch(propertyInfo.Name)
			{
				case nameof(Order.PaymentType):
					_resultOrder.UpdatePaymentType(_copiedOrder.PaymentType, _contractUpdater);
					break;
				case nameof(Order.Client):
					_resultOrder.UpdateClient(_copiedOrder.Client, _contractUpdater, out var message);
					break;
				case nameof(Order.DeliveryPoint):
					_resultOrder.UpdateDeliveryPoint(_copiedOrder.DeliveryPoint, _contractUpdater);
					break;
				case nameof(Order.DeliveryDate):
					_resultOrder.UpdateDeliveryDate(_copiedOrder.DeliveryDate, _contractUpdater, out var updateDeliveryDateMessage);
					break;
				default:
					var value = propertyInfo.GetValue(_copiedOrder);
					propertyInfo.SetValue(_resultOrder, value);
					break;
			}
		}

		/// <summary>
		/// Копирует документы которые были добавлены из других заказов.
		/// Документы которые должны быть добавлены для текущего заказа, формируются автоматически!
		/// </summary>
		public CopyingOrder CopyAttachedDocuments()
		{
			var attachedDocuments = _copiedOrder.OrderDocuments
				.Where(x => x.AttachedToOrder != null)
				.Where(x => x.Order.Id != _copiedOrder.Id);

			_resultOrder.AddAdditionalDocuments(attachedDocuments);

			return this;
		}

		/// <summary>
		/// Копирование товаров (<see cref="OrderItem"/>) заказа и связанного с ним оборудования (<see cref="OrderEquipment"/>)
		/// </summary>
		/// <param name="withDiscounts">true - копируем со скидками false - не переносим скидки</param>
		/// <param name="withPrices">true - ставим ценам флаг ручного изменения, чтобы они были неизменны
		/// false - выставляем флаг ручной цены из копируемого заказа</param>
		/// <param name="isCopiedFromUndelivery">true, если копируем из недовоза, false - если копируем не из недовоза</param>
		public CopyingOrder CopyOrderItems(bool withDiscounts = false, bool withPrices = false, bool isCopiedFromUndelivery = false)
		{
			var orderItems = _copiedOrder.OrderItems
				.Where(x => x.PromoSet == null)
				.Where(x => x.Nomenclature.Id != _paidDeliveryNomenclatureId)
				.Where(x => x.Nomenclature.Id != _fastDeliveryNomenclatureId)
				.Where(x => x.Nomenclature.Id != _masterCallNomenclatureId);

			foreach(var orderItem in orderItems)
			{
				CopyOrderItem(orderItem, withDiscounts, withPrices, isCopiedFromUndelivery);
				CopyDependentOrderEquipment(orderItem);
			}

			_resultOrder.RecalculateItemsPrice();

			return this;
		}

		/// <summary>
		/// Копирование товаров (<see cref="OrderItem"/>) заказа за исключением тех,
		/// которые связаны с оборудованием (например, позиции аренды оборудования) (<see cref="OrderEquipment"/>)
		/// </summary>
		/// <param name="withDiscounts">true - копируем со скидками false - не переносим скидки</param>
		/// <param name="withPrices">true - ставим ценам флаг ручного изменения, чтобы они были неизменны
		/// false - выставляем флаг ручной цены из копируемого заказа</param>
		/// <param name="isCopiedFromUndelivery">true, если копируем из недовоза, false - если копируем не из недовоза</param>
		public CopyingOrder CopyOrderItemsExceptEquipmentReferenced(bool withDiscounts = false, bool withPrices = false, bool isCopiedFromUndelivery = false)
		{
			var equipmentReferencedOrderItems = GetEquipmentReferencedOrderItems();

			var orderItems = _copiedOrder.OrderItems
				.Where(x => x.PromoSet == null)
				.Where(x => x.Nomenclature.Id != _paidDeliveryNomenclatureId)
				.Where(x => x.Nomenclature.Id != _fastDeliveryNomenclatureId)
				.Where(x => x.Nomenclature.Id != _masterCallNomenclatureId)
				.Where(x => !equipmentReferencedOrderItems.Contains(x));

			foreach(var orderItem in orderItems)
			{
				CopyOrderItem(orderItem, withDiscounts, withPrices, isCopiedFromUndelivery);
				CopyDependentOrderEquipment(orderItem);
			}

			_resultOrder.RecalculateItemsPrice();

			return this;
		}

		private IEnumerable<OrderItem> GetEquipmentReferencedOrderItems()
		{
			var orderRentDepositItems = _copiedOrder.OrderEquipments
				.Where(x => x.OrderRentDepositItem != null)
				.Select(x => x.OrderRentDepositItem);

			var orderRentServiceItem = _copiedOrder.OrderEquipments
				.Where(x => x.OrderRentServiceItem != null)
				.Select(x => x.OrderRentServiceItem);

			var equipmentReferencedItems = _copiedOrder.OrderEquipments
				.Where(x => x.OrderItem != null)
				.Select(x => x.OrderItem);

			return orderRentDepositItems.Concat(orderRentServiceItem).Concat(equipmentReferencedItems);
		}

		/// <summary>
		/// Копирование доставки. При переносе из недовоза платная доставка должна переносится со всеми скидками и старой ценой.
		/// Использовать только после выполнения (<see cref="CopyOrderItems"/>)
		/// </summary>
		public CopyingOrder CopyPaidDeliveryItem()
		{
			var paidDeliveryFromCopiedOrder =
				_copiedOrder.OrderItems.SingleOrDefault(x => x.Nomenclature.Id == _paidDeliveryNomenclatureId);

			var currentPaidDelivery = _resultOrder.OrderItems.SingleOrDefault(x => x.Nomenclature.Id == _paidDeliveryNomenclatureId);
			if(currentPaidDelivery != null)
			{
				_resultOrder.OrderItems.Remove(currentPaidDelivery);
			}
			if(paidDeliveryFromCopiedOrder != null)
			{
				CopyOrderItem(paidDeliveryFromCopiedOrder, true, true, true);
			}

			return this;
		}

		/// <summary>
		/// Копирование оборудования не связанного с товарами (<see cref="OrderItem"/>) заказа.
		/// Обрудование связанное с товарами (<see cref="OrderItem"/>) переносится вместе с товарами (<see cref="OrderItem"/>)
		/// </summary>
		public CopyingOrder CopyAdditionalOrderEquipments()
		{
			var orderEquipments = _copiedOrder.OrderEquipments
				.Where(x => !_flyersNomenclaturesIds.Contains(x.Nomenclature.Id))
				.Where(x => x.OrderItem == null);

			foreach(var orderEquipment in orderEquipments)
			{
				CopyOrderEquipment(orderEquipment);
			}

			return this;
		}

		/// <summary>
		/// Копирование возвратов залогов (<see cref="OrderDepositItem"/>)
		/// </summary>
		public CopyingOrder CopyOrderDepositItems()
		{
			foreach(var depositItem in _copiedOrder.OrderDepositItems)
			{
				var newDepositItem = new OrderDepositItem
				{
					Order = _resultOrder,
					Count = depositItem.Count,
					Deposit = depositItem.Deposit,
					DepositType = depositItem.DepositType,
					EquipmentNomenclature = depositItem.EquipmentNomenclature
				};

				_resultOrder.ObservableOrderDepositItems.Add(newDepositItem);
			}

			return this;
		}

		/// <summary>
		/// Копирование данных об оплате по карте
		/// </summary>
		public CopyingOrder CopyPaymentByCardDataIfPossible()
		{
			if(_copiedOrder.PaymentType != PaymentType.PaidOnline)
			{
				return this;
			}

			_resultOrder.OnlinePaymentNumber = _copiedOrder.OnlinePaymentNumber;
			_resultOrder.UpdatePaymentByCardFrom(_copiedOrder.PaymentByCardFrom, _contractUpdater);

			return this;
		}

		/// <summary>
		/// Копирование данных об оплате по QR коду из приложения водителя или СМС
		/// </summary>
		public CopyingOrder CopyPaymentByQrDataIfPossible()
		{
			if(_copiedOrder.PaymentType != PaymentType.DriverApplicationQR
				&& _copiedOrder.PaymentType != PaymentType.SmsQR)
			{
				return this;
			}

			_resultOrder.OnlinePaymentNumber = _copiedOrder.OnlinePaymentNumber;

			return this;
		}

		/// <summary>
		/// Копирование промонаборов <see cref="PromotionalSet"/> и связанных с ними товаров <see cref="OrderItem"/>
		/// </summary>
		public CopyingOrder CopyPromotionalSets()
		{
			foreach(var proSet in _copiedOrder.PromotionalSets)
			{
				_resultOrder.ObservablePromotionalSets.Add(proSet);
			}

			var orderItems = _copiedOrder.OrderItems
				.Where(x => x.PromoSet != null)
				.Where(x => x.Nomenclature.Id != _paidDeliveryNomenclatureId);

			foreach(var promosetOrderItem in orderItems)
			{
				CopyOrderItem(promosetOrderItem, true, _needCopyStockBottleDiscount, _needCopyStockBottleDiscount);
				CopyDependentOrderEquipment(promosetOrderItem);
			}

			return this;
		}

		public Order GetResult()
		{
			_resultOrder.UpdateDocuments();
			return _resultOrder;
		}

		private void CopyDependentOrderEquipment(OrderItem orderItem)
		{
			var orderEquipments = _copiedOrder.OrderEquipments
				.Where(x => !_flyersNomenclaturesIds.Contains(x.Nomenclature.Id))
				.Where(x => x.OrderItem != null)
				.Where(x => x.OrderItem.Id == orderItem.Id);

			foreach(var orderEquipment in orderEquipments)
			{
				CopyOrderEquipment(orderEquipment);
			}
		}

		private void CopyOrderItem(
			OrderItem orderItem,
			bool withDiscounts = false,
			bool withPrices = false,
			bool isCopiedFromUndelivery = false)
		{
			var newOrderItem = OrderItem.CreateForSale(_resultOrder, orderItem.Nomenclature, orderItem.Count, orderItem.Price);
			
			newOrderItem.PromoSet = orderItem.PromoSet;
			newOrderItem.IsUserPrice = withPrices;
			newOrderItem.IsAlternativePrice = orderItem.IsAlternativePrice;
			newOrderItem.IncludeNDS = orderItem.IncludeNDS;

			//если перенос из недовоза - сохраняем ссылку на переносимый товар
			if(isCopiedFromUndelivery)
			{
				newOrderItem.CopiedFromUndelivery = orderItem;
			}
			if(withDiscounts)
			{
				CopyingDiscounts(orderItem, newOrderItem, _needCopyStockBottleDiscount);
			}

			_resultOrder.AddOrderItem(_uow, _contractUpdater, newOrderItem);
		}

		private void CopyingDiscounts(OrderItem orderItemFrom, OrderItem orderItemTo, bool withStockBottleDiscount)
		{
			var isPromoset = orderItemFrom.PromoSet != null;

			if(orderItemFrom.DiscountMoney > 0 && orderItemFrom.Discount > 0 && (orderItemFrom.DiscountReason != null || isPromoset))
			{
				orderItemTo.SetDiscount(orderItemFrom.IsDiscountInMoney, orderItemFrom.Discount, orderItemFrom.DiscountMoney, orderItemFrom.DiscountReason);
			}
			else if(orderItemFrom.OriginalDiscountMoney > 0 && orderItemFrom.OriginalDiscount > 0 && (orderItemFrom.OriginalDiscountReason != null || isPromoset))
			{
				orderItemTo.SetDiscount(orderItemFrom.IsDiscountInMoney, orderItemFrom.OriginalDiscount.Value, orderItemFrom.OriginalDiscountMoney.Value, orderItemFrom.OriginalDiscountReason);
			}

			if(withStockBottleDiscount)
			{
				orderItemTo.DiscountByStock = orderItemFrom.DiscountByStock;
			}
		}

		private void CopyOrderEquipment(OrderEquipment orderEquipment)
		{
			var newOrderEquipment = new OrderEquipment
			{
				Order = _resultOrder,
				Direction = orderEquipment.Direction,
				DirectionReason = orderEquipment.DirectionReason,
				OrderItem = orderEquipment.OrderItem,
				Equipment = orderEquipment.Equipment,
				OwnType = orderEquipment.OwnType,
				Nomenclature = orderEquipment.Nomenclature,
				Reason = orderEquipment.Reason,
				Confirmed = orderEquipment.Confirmed,
				ConfirmedComment = orderEquipment.ConfirmedComment,
				Count = orderEquipment.Count
			};

			_resultOrder.ObservableOrderEquipments.Add(newOrderEquipment);
		}
	}
}
