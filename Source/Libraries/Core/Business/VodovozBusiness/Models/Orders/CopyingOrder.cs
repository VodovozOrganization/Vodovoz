using Gamma.Utilities;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Services;

namespace Vodovoz.Models.Orders
{
	public class CopyingOrder
	{
		private readonly IUnitOfWork _uow;
		private readonly Order _copiedOrder;
		private readonly Order _resultOrder;
		private readonly INomenclatureParametersProvider _nomenclatureParametersProvider;
		private readonly IFlyerRepository _flyerRepository;
		private readonly int _paidDeliveryNomenclatureId;
		private readonly IList<int> _flyersNomenclaturesIds;
		private readonly int _fastDeliveryNomenclatureId;

		private bool _needCopyStockBottleDiscount;

		internal CopyingOrder(IUnitOfWork uow, Order copiedOrder, Order resultOrder,
			INomenclatureParametersProvider nomenclatureParametersProvider, IFlyerRepository flyerRepository)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_copiedOrder = copiedOrder ?? throw new ArgumentNullException(nameof(copiedOrder));
			_resultOrder = resultOrder ?? throw new ArgumentNullException(nameof(resultOrder));
			if(resultOrder.Id > 0)
			{
				throw new ArgumentException(
					$"Заказ, в который переносятся данные из копируемого заказа, должен быть новым. (Свойство {nameof(resultOrder.Id)} должно быть равно 0)");
			}

			_nomenclatureParametersProvider =
				nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));

			_paidDeliveryNomenclatureId = _nomenclatureParametersProvider.PaidDeliveryNomenclatureId;
			_flyersNomenclaturesIds = _flyerRepository.GetAllFlyersNomenclaturesIds(_uow);
			_fastDeliveryNomenclatureId = _nomenclatureParametersProvider.FastDeliveryNomenclatureId;
		}

		public Order GetCopiedOrder => _copiedOrder;

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
				_resultOrder.Client = _copiedOrder.Client;
				_resultOrder.SelfDelivery = _copiedOrder.SelfDelivery;
				_resultOrder.DeliveryPoint = _copiedOrder.DeliveryPoint;
				_resultOrder.PaymentType = _copiedOrder.PaymentType;
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

			var value = propertyInfo.GetValue(_copiedOrder);
			propertyInfo.SetValue(_resultOrder, value);
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
		/// <param name="forceUseAlternativePrice">Использование альтернативной цены номенклатуры</param>
		public CopyingOrder CopyOrderItems(bool withDiscounts = false, bool withPrices = false)
		{
			var orderItems = _copiedOrder.OrderItems
				.Where(x => x.PromoSet == null)
				.Where(x => x.Nomenclature.Id != _paidDeliveryNomenclatureId)
				.Where(x => x.Nomenclature.Id != _fastDeliveryNomenclatureId);

			foreach(var orderItem in orderItems)
			{
				CopyOrderItem(orderItem, withDiscounts, withPrices);
				CopyDependentOrderEquipment(orderItem);
			}

			_resultOrder.RecalculateItemsPrice();

			return this;
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
				CopyOrderItem(paidDeliveryFromCopiedOrder, true, true);
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

			_resultOrder.OnlineOrder = _copiedOrder.OnlineOrder;
			_resultOrder.PaymentByCardFrom = _copiedOrder.PaymentByCardFrom;

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

			_resultOrder.OnlineOrder = _copiedOrder.OnlineOrder;

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
				CopyOrderItem(promosetOrderItem, true, _needCopyStockBottleDiscount);
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
			bool withPrices = false)
		{
			var newOrderItem = new OrderItem
			{
				Order = _resultOrder,
				Nomenclature = orderItem.Nomenclature,
				PromoSet = orderItem.PromoSet,
				Price = orderItem.Price,
				IsUserPrice = withPrices,
				IsAlternativePrice = orderItem.IsAlternativePrice,
				Count = orderItem.Count,
				IncludeNDS = orderItem.IncludeNDS
			};

			//если перенос из недовоза - сохраняем ссылку на переносимый товар
			if(withPrices)
			{
				newOrderItem.CopiedFromUndelivery = orderItem;
			}
			if(withDiscounts)
			{
				CopyingDiscounts(orderItem, newOrderItem, _needCopyStockBottleDiscount);
			}

			_resultOrder.AddOrderItem(newOrderItem);
		}

		private void CopyingDiscounts(OrderItem orderItemFrom, OrderItem orderItemTo, bool withStockBottleDiscount)
		{
			var isPromoset = orderItemFrom.PromoSet != null;

			if(orderItemFrom.DiscountMoney > 0 && orderItemFrom.Discount > 0 && (orderItemFrom.DiscountReason != null || isPromoset))
			{
				orderItemTo.IsDiscountInMoney = orderItemFrom.IsDiscountInMoney;
				orderItemTo.Discount = orderItemFrom.Discount;
				orderItemTo.DiscountMoney = orderItemFrom.DiscountMoney;
				orderItemTo.DiscountReason = orderItemFrom.DiscountReason;
			}
			else if(orderItemFrom.OriginalDiscountMoney > 0 && orderItemFrom.OriginalDiscount > 0 && (orderItemFrom.OriginalDiscountReason != null || isPromoset))
			{
				orderItemTo.IsDiscountInMoney = orderItemFrom.IsDiscountInMoney;
				orderItemTo.Discount = orderItemFrom.OriginalDiscount.Value;
				orderItemTo.DiscountMoney = orderItemFrom.OriginalDiscountMoney.Value;
				orderItemTo.DiscountReason = orderItemFrom.OriginalDiscountReason;
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
