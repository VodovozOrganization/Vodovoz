using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Models.Orders
{
	public class PartitionedOrder
	{
		private readonly IUnitOfWork _uow;
		private readonly Order _copiedOrder;
		private readonly Order _resultOrder;
		private readonly IOrderContractUpdater _contractUpdater;
		private readonly int _paidDeliveryNomenclatureId;
		private readonly IList<int> _flyersNomenclaturesIds;
		private bool _needCopyStockBottleDiscount;

		public PartitionedOrder(
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
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));

			if(nomenclatureSettings is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureSettings));
			}
			
			if(flyerRepository is null)
			{
				throw new ArgumentNullException(nameof(flyerRepository));
			}

			_paidDeliveryNomenclatureId = nomenclatureSettings.PaidDeliveryNomenclatureId;
			_flyersNomenclaturesIds = flyerRepository.GetAllFlyersNomenclaturesIds(_uow);
		}
		
		/// <summary>
		/// Копирование основных полей заказа
		/// </summary>
		public PartitionedOrder CopyFields()
		{
			_resultOrder.UpdateClient(_copiedOrder.Client, _contractUpdater, out var updateClientMessage);
			_resultOrder.SelfDelivery = _copiedOrder.SelfDelivery;
			_resultOrder.UpdateDeliveryPoint(_copiedOrder.DeliveryPoint, _contractUpdater);
			_resultOrder.UpdatePaymentType(_copiedOrder.PaymentType, _contractUpdater);
			_resultOrder.UpdateDeliveryDate(_copiedOrder.DeliveryDate, _contractUpdater, out var updateDeliveryDateMessage);
			_resultOrder.UpdatePaymentByCardFrom(_copiedOrder.PaymentByCardFrom, _contractUpdater);
			_resultOrder.OnlinePaymentNumber = _copiedOrder.OnlinePaymentNumber;
			_resultOrder.DeliverySchedule = _copiedOrder.DeliverySchedule;
			_resultOrder.Author = _copiedOrder.Author;
			_resultOrder.Comment = _copiedOrder.Comment;
			_resultOrder.HasCommentForDriver = _copiedOrder.HasCommentForDriver;
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
			_resultOrder.CallBeforeArrivalMinutes = _copiedOrder.CallBeforeArrivalMinutes;
			_resultOrder.IsDoNotMakeCallBeforeArrival = _copiedOrder.IsDoNotMakeCallBeforeArrival;
			_resultOrder.SelfDeliveryGeoGroup = _copiedOrder.SelfDeliveryGeoGroup;

			return this;
		}
		
		/// <summary>
		/// Копирование полей, связанных с акцией бутыль
		/// </summary>
		public PartitionedOrder CopyStockBottle()
		{
			_resultOrder.IsBottleStock = _copiedOrder.IsBottleStock;
			_resultOrder.BottlesByStockCount = _copiedOrder.BottlesByStockCount;
			_needCopyStockBottleDiscount = true;

			return this;
		}
		
		/// <summary>
		/// Копирует документы, которые были добавлены из других заказов.
		/// Документы которые должны быть добавлены для текущего заказа, формируются автоматически!
		/// </summary>
		public PartitionedOrder CopyAttachedDocuments()
		{
			var attachedDocuments = _copiedOrder.OrderDocuments
				.Where(x => x.AttachedToOrder != null)
				.Where(x => x.Order.Id != _copiedOrder.Id);

			_resultOrder.AddAdditionalDocuments(attachedDocuments);

			return this;
		}
		
		/// <summary>
		/// Очистка товаров, оборудования и залогов
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public PartitionedOrder ClearGoodsAndEquipmentsAndDeposits()
		{
			_resultOrder.ObservablePromotionalSets.Clear();
			_resultOrder.ObservableOrderItems.Clear();
			_resultOrder.ObservableOrderEquipments.Clear();
			_resultOrder.ObservableOrderDepositItems.Clear();
			
			return this;
		}
		
		/// <summary>
		/// Копирование товаров (<see cref="OrderItem"/>) заказа
		/// </summary>
		/// <param name="withDiscounts">true - копируем со скидками false - не переносим скидки</param>
		/// <param name="copyingItems">Копируемые товары заказа</param>
		public PartitionedOrder CopyOrderItems(IEnumerable<IProduct> copyingItems, bool withDiscounts = false)
		{
			var goods = copyingItems
				.Where(x => x.PromoSet == null);

			CopyGoods(withDiscounts, goods);

			_resultOrder.RecalculateItemsPrice();

			return this;
		}

		private void CopyGoods(bool withDiscounts, IEnumerable<IProduct> goods)
		{
			foreach(var product in goods)
			{
				switch(product)
				{
					case OrderItem orderItem:
						CopyOrderItem(orderItem, withDiscounts);
						break;
					default:
						throw new InvalidOperationException("Копирование товаров рассчитано только на товары сущности заказ!");
				}
			}
		}

		/// <summary>
		/// Копирование оборудования для части заказа <see cref="OrderEquipment"/>
		/// </summary>
		public PartitionedOrder CopyOrderEquipments(IEnumerable<OrderEquipment> orderEquipments)
		{
			if(orderEquipments is null)
			{
				return this;
			}

			orderEquipments = orderEquipments.Where(x => !_flyersNomenclaturesIds.Contains(x.Nomenclature.Id));
			AddOrderEquipments(orderEquipments);

			return this;
		}

		/// <summary>
		/// Копирование возвратов залогов (<see cref="OrderDepositItem"/>)
		/// </summary>
		public PartitionedOrder CopyOrderDepositItems(IEnumerable<OrderDepositItem> orderDepositItems)
		{
			if(orderDepositItems is null)
			{
				return this;
			}
			
			AddDepositItems(orderDepositItems);

			return this;
		}
		
		/// <summary>
		/// Копирование промонаборов <see cref="PromotionalSet"/> и связанных с ними товаров <see cref="OrderItem"/>
		/// <param name="copyingItems">Копируемые товары заказа</param>
		/// </summary>
		public PartitionedOrder CopyPromotionalSets(IEnumerable<IProduct> copyingItems)
		{
			var promoSets =
				(from copyingItem in copyingItems
					where copyingItem.PromoSet != null
					select copyingItem.PromoSet)
				.ToList();

			foreach(var promoSet in promoSets)
			{
				_resultOrder.ObservablePromotionalSets.Add(promoSet);
			}

			var goods = copyingItems
				.Where(x => x.PromoSet != null)
				.Where(x => x.Nomenclature.Id != _paidDeliveryNomenclatureId);

			CopyGoods(true, goods);

			return this;
		}

		private void CopyOrderItem(
			OrderItem orderItem,
			bool withDiscounts = false)
		{
			var newOrderItem = OrderItem.CreateForSale(_resultOrder, orderItem.Nomenclature, orderItem.Count, orderItem.Price);
			
			newOrderItem.PromoSet = orderItem.PromoSet;
			newOrderItem.IsAlternativePrice = orderItem.IsAlternativePrice;
			newOrderItem.IncludeNDS = orderItem.IncludeNDS;
			
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

		private void AddOrderEquipments(IEnumerable<OrderEquipment> orderEquipments)
		{
			foreach(var orderEquipment in orderEquipments)
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
		
		private void AddDepositItems(IEnumerable<OrderDepositItem> orderDepositItems)
		{
			foreach(var depositItem in orderDepositItems)
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
		}
	}
}
