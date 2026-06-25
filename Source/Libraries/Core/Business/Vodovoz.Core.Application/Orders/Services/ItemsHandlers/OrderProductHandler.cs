using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Factories;
using VodovozBusiness.Handlers;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Core.Application.Orders.Services.ItemsHandlers
{
	public class OrderProductHandler : ProductHandler, IOrderProductHandler
	{
		private readonly IOrderContractUpdater _contractUpdater;

		public OrderProductHandler(
			IOrderContractUpdater contractUpdater,
			ISaleItemFactory saleItemFactory,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			INomenclatureRepository nomenclatureRepository,
			IGoodsPriceCalculator goodsPriceCalculator,
			IFixedPriceHandler fixedPriceHandler,
			IAddProductValidator addProductValidator
			)
			: base(
				saleItemFactory,
				nomenclatureSettings,
				nomenclatureService,
				nomenclatureRepository,
				goodsPriceCalculator,
				fixedPriceHandler,
				addProductValidator
			)
		{
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
		}

		protected Order Order => SaleItemSource?.Source as Order;

		public override Result TryAddProduct(
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			IEnumerable<DiscountReason> discountReasons = null
			)
		{
			var addProductResult = base.TryAddProduct(nomenclature, count, discount, discountReasons);

			if(addProductResult.IsSuccess)
			{
				_contractUpdater.UpdateContract(UoW, Order);
			}
			
			return addProductResult;
		}

		#region Промонаборы

		public override void ActivatePromotionalSet(PromotionalSet proSet)
		{
			//Добавление спец. действий промонабора
			foreach(var action in proSet.PromotionalSetActions)
			{
				action.Activate(Order);
			}

			base.TryAddNomenclatureFromPromoSet(proSet);
			Order.ObservablePromotionalSets.Add(proSet);
		}
		
		/// <summary>
		/// Попытка найти и удалить промонабор, если нет больше позиций
		/// заказа с промонабором
		/// </summary>
		/// <param name="saleItem">Позиция заказа</param>
		public override void TryToRemovePromotionalSet(IProduct saleItem)
		{
			var proSetFromOrderItem = saleItem.PromoSet;
			if(proSetFromOrderItem != null)
			{
				var proSetToRemove = Order.ObservablePromotionalSets.FirstOrDefault(s => s == proSetFromOrderItem);
				if(proSetToRemove != null && !SaleItemSource.Products.Any(i => i.PromoSet == proSetToRemove))
				{
					foreach(var action in proSetToRemove.ObservablePromotionalSetActions)
					{
						action.Deactivate(Order);
					}

					Order.ObservablePromotionalSets.Remove(proSetToRemove);
				}
			}
		}

		#endregion

		#region Аренда
		
		public override void AddNonFreeRent(
			PaidRentPackage paidRentPackage,
			Nomenclature equipmentNomenclature
			)
		{
			var orderRentDepositItem = AddNonFreeRentDepositItems(paidRentPackage, out var orderRentServiceItem);

			var orderRentEquipment = GetExistingRentEquipmentItem(
				equipmentNomenclature,
				orderRentDepositItem as OrderItem,
				orderRentServiceItem as OrderItem
				);
			
			if(orderRentEquipment == null)
			{
				orderRentEquipment = CreateNewRentEquipmentItem(
					equipmentNomenclature,
					orderRentDepositItem as OrderItem,
					orderRentServiceItem as OrderItem
					);
				
				Order.ObservableOrderEquipments.Add(orderRentEquipment);
			}
			else
			{
				orderRentEquipment.Count++;
			}

			Order.UpdateRentsCount();
		}
		
		public override void AddDailyRent(
			PaidRentPackage paidRentPackage,
			Nomenclature equipmentNomenclature
			)
		{
			var orderRentDepositItem = AddDailyRentDepositItems(paidRentPackage, out var orderRentServiceItem);

			var orderRentEquipment = GetExistingRentEquipmentItem(
				equipmentNomenclature,
				orderRentDepositItem as OrderItem,
				orderRentServiceItem as OrderItem
				);
			
			if(orderRentEquipment == null)
			{
				orderRentEquipment = CreateNewRentEquipmentItem(
					equipmentNomenclature,
					orderRentDepositItem as OrderItem,
					orderRentServiceItem as OrderItem
					);
				
				Order.ObservableOrderEquipments.Add(orderRentEquipment);
			}
			else
			{
				orderRentEquipment.Count++;
			}

			Order.UpdateRentsCount();
		}

		public override void AddFreeRent(
			FreeRentPackage freeRentPackage,
			Nomenclature equipmentNomenclature
			)
		{
			var orderRentDepositItem = AddFreeRentDepositItem(freeRentPackage);

			var orderRentEquipment = GetExistingRentEquipmentItem(equipmentNomenclature, orderRentDepositItem as OrderItem);
			if(orderRentEquipment is null)
			{
				orderRentEquipment = CreateNewRentEquipmentItem(equipmentNomenclature, orderRentDepositItem as OrderItem);
				Order.ObservableOrderEquipments.Add(orderRentEquipment);
			}
			else
			{
				orderRentEquipment.Count++;
			}

			Order.UpdateRentsCount();
		}

		#endregion

		public override void RemoveSaleItem(IProduct removableProduct)
		{
			base.RemoveSaleItem(removableProduct);
			
			//Если была удалена последняя номенклатура "мастер" - переходит в стандартный тип адреса
			if(SaleItemSource.Products.All(x => !(x.IsMasterNomenclature && x.Nomenclature.Id != NomenclatureSettings.MasterCallNomenclatureId))
				&& removableProduct.IsMasterNomenclature
				&& removableProduct.Nomenclature.Id != NomenclatureSettings.MasterCallNomenclatureId)
			{
				Order.OrderAddressType = OrderAddressType.Delivery;
			}

			_contractUpdater.UpdateContract(UoW, Order);
		}
		
		public virtual void RemoveItemFromClosingOrder(IProduct removableItem)
		{
			if((removableItem.Count != 0 && removableItem.Price != 0)
				|| Order.OrderEquipments.Any(x => x.OrderItem == removableItem))
			{
				return;
			}

			RemoveSaleItem(removableItem);
		}

		public override void RemoveItem(IProduct removableItem)
		{
			base.RemoveItem(removableItem);
			
			DeleteOrderEquipmentOnOrderItem(Order, removableItem as OrderItem);
			Order.UpdateDocuments();
		}
		
		public virtual void RemoveEquipment(OrderEquipment item)
		{
			var rentDepositOrderItem = item.OrderRentDepositItem;
			var rentServiceOrderItem = item.OrderRentServiceItem;
			var totalEquipmentCountForDeposit = 0;
			var totalEquipmentCountForService = 0;

			if(rentDepositOrderItem != null)
			{
				totalEquipmentCountForDeposit = Order.GetRentEquipmentTotalCountForDepositItem(rentDepositOrderItem);
			}
			
			if(rentServiceOrderItem != null)
			{
				totalEquipmentCountForService = Order.GetRentEquipmentTotalCountForServiceItem(rentServiceOrderItem);
			}

			if(totalEquipmentCountForDeposit == item.Count || totalEquipmentCountForService == item.Count)
			{
				Order.ObservableOrderEquipments.Remove(item);
				RemoveSaleItem(rentDepositOrderItem);
				RemoveSaleItem(rentServiceOrderItem);
			}
			else
			{
				Order.ObservableOrderEquipments.Remove(item);
				Order.UpdateRentsCount();
			}

			Order.UpdateDocuments();
		}

		/// <summary>
		/// Удаляет оборудование в заказе, связанное с товаром в заказе
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <param name="removedItem">Товар в заказе по которому будет удалятся оборудование</param>
		private void DeleteOrderEquipmentOnOrderItem(Order order, OrderItem removedItem)
		{
			var orderEquipments = order.ObservableOrderEquipments
					.Where(x => x.OrderItem == removedItem)
					.ToList();
			foreach(var orderEquipment in orderEquipments)
			{
				order.ObservableOrderEquipments.Remove(orderEquipment);
			}
		}
		
		private OrderEquipment GetExistingRentEquipmentItem(
			Nomenclature nomenclature,
			OrderItem rentDepositItem,
			OrderItem rentServiceItem = null)
		{
			var rentEquipment = Order.OrderEquipments
				.Where(x => x.Reason == Reason.Rent)
				.Where(x => x.Nomenclature == nomenclature)
				.Where(x => x.OrderRentDepositItem == rentDepositItem)
				.FirstOrDefault(x => x.OrderRentServiceItem == rentServiceItem);
			return rentEquipment;
		}
		
		private OrderEquipment CreateNewRentEquipmentItem(
			Nomenclature nomenclature,
			OrderItem rentDepositItem,
			OrderItem rentServiceItem = null)
		{
			var rentEquipment = new OrderEquipment
			{
				Order = Order,
				Count = 1,
				Direction = Direction.Deliver,
				Nomenclature = nomenclature,
				Reason = Reason.Rent,
				DirectionReason = DirectionReason.Rent,
				OwnType = OwnTypes.Rent,
				OrderRentDepositItem = rentDepositItem,
				OrderRentServiceItem = rentServiceItem
			};
			
			return rentEquipment;
		}
	}
}
