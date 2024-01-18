using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Parameters;

namespace Vodovoz.Tools.Orders
{
	public class OrderStateKey
	{
		[Display(Name = "Заказ, для которого определяются параметры")]
		public Order Order { get; set; }

		public OrderStateKey() { }

		public OrderStateKey(Order order)
		{
			Order = order;
			OrderStatus = Order.OrderStatus;
			InitializeFields();
		}

		/// <summary>
		/// Создает ключ для определенного требуемого статуса
		/// </summary>
		public OrderStateKey(Order order, OrderStatus requiredStatus)
		{
			Order = order;
			OrderStatus = requiredStatus;
			InitializeFields();
		}

		#region для проверки документов

		[Display(Name = "Есть ли товары в заказе?")]
		public bool HasOrderItems { get; set; } = false;

		[Display(Name = "Есть ли оборудование в заказе?")]
		public bool HasOrderEquipment { get; set; } = false;

		[Display(Name = "Нужен ли возврат залога от клиента?")]
		public bool NeedToRefundDepositToClient { get; set; } = false;

		[Display(Name = "Есть ли возврат тары?")]
		public bool NeedToReturnBottles { get; set; } = false;

		[Display(Name = "Стоимость товаров заказа равна нулю?")]
		public bool IsPriceOfAllOrderItemsZero { get; set; } = false;

		[Display(Name = "Тип документа 'ТОРГ12 + Счёт-фактура'?")]
		private bool IsDocTypeTORG12 { get; set; } = false;

		[Display(Name = "Есть выезд мастера?")]
		public bool NeedMaster { get; set; } = false;

		[Display(Name = "Самовывоз?")]
		public bool IsSelfDelivery { get; set; } = false;

		[Display(Name = "Оплата после отгрузки")]
		public bool PayAfterShipment { get; internal set; }

		[Display(Name = "Статус заказа")]
		public OrderStatus OrderStatus { get; set; }

		[Display(Name = "Тип оплаты")]
		public PaymentType PaymentType { get; set; }

		[Display(Name = "Есть ли специальные поля для печати в контрагенте?")]
		public bool HaveSpecialFields { get; set; }

		[Display(Name = "Тип документа")]
		public DefaultDocumentType? DefaultDocumentType { get; set; } = Domain.Client.DefaultDocumentType.upd;

		[Display(Name = "Заказ интернет магазина ?")]
		public bool HasEShopOrder { get; set; } = false;

		#endregion

		#region для проверки цены доставки

		[Display(Name = "Сколько воды многооборотной таре 19л?")]
		public decimal NotDisposableWater19LCount { get; set; }

		[Display(Name = "Сколько воды одноразовой таре 19л?")]
		public decimal DisposableWater19LCount { get; set; }

		[Display(Name = "Сколько воды одноразовой таре 6л?")]
		public decimal DisposableWater6LCount { get; set; }
		
		[Display(Name = "Сколько воды одноразовой таре 1.5л?")]
		public decimal DisposableWater1500mlCount { get; set; }

		[Display(Name = "Сколько воды одноразовой таре 0.6л?")]
		public decimal DisposableWater600mlCount { get; set; }
		
		[Display(Name = "Сколько воды одноразовой таре 0.5л?")]
		public decimal DisposableWater500mlCount { get; set; }

		#endregion

		public IEnumerable<OrderEquipment> OnlyEquipments => Order.OrderEquipments.Where(x => x.Nomenclature.Category == Domain.Goods.NomenclatureCategory.equipment);

		void InitializeFields()
		{
			//для документов
			DefaultDocumentType = Order.DocumentType ?? Order.Client.DefaultDocumentType;
			IsDocTypeTORG12 = DefaultDocumentType.HasValue && DefaultDocumentType == Domain.Client.DefaultDocumentType.torg12;

			HasOrderEquipment = HasOrderEquipments(Order.UoW);

			if(!Order.ObservableOrderItems.Any() || 
			   (Order.ObservableOrderItems.Count == 1 && Order.ObservableOrderItems.Any(x => 
				   x.Nomenclature.Id == int.Parse(new ParametersProvider().GetParameterValue("paid_delivery_nomenclature_id"))))) 
			{
				HasOrderItems = false;
			}
			else
			{
				HasOrderItems = true;
			}
			
			IsPriceOfAllOrderItemsZero = Order.ObservableOrderItems.Sum(i => i.ActualSum) <= 0m;
			NeedToReturnBottles = Order.BottlesReturn > 0;
			NeedToRefundDepositToClient = Order.ObservableOrderDepositItems.Any();
			PaymentType = Order.PaymentType;
			HaveSpecialFields = Order.Client.UseSpecialDocFields;
			NeedMaster = Order.OrderItems.Any(i => i.Nomenclature.Category == Domain.Goods.NomenclatureCategory.master);
			IsSelfDelivery = Order.SelfDelivery;
			PayAfterShipment = Order.PayAfterShipment;
			HasEShopOrder = Order.EShopOrder.HasValue;

			//для проверки цены доставки
			CalculateAllWaterCount();
		}

		private void CalculateAllWaterCount()
		{
			CalculatePromoSetWaterCount();
			CalculateNotPromoSetWaterCount();
		}

		private void CalculatePromoSetWaterCount()
		{
			var water = Order.OrderItems.Where(
				x => x.PromoSet != null &&
					x.Nomenclature != null &&
					x.Nomenclature.Category == NomenclatureCategory.water)
				.ToList();

			foreach(var item in water)
			{
				if(item.PromoSet.BottlesCountForCalculatingDeliveryPrice.HasValue)
				{
					NotDisposableWater19LCount = item.PromoSet.BottlesCountForCalculatingDeliveryPrice.Value;
					break;
				}
				
				CalculateWaterCount(item);
			}
		}

		private void CalculateNotPromoSetWaterCount()
		{
			var water = Order.OrderItems.Where(
				x => x.PromoSet == null &&
				x.Nomenclature != null &&
				x.Nomenclature.Category == NomenclatureCategory.water)
				.ToList();

			foreach(var item in water)
			{
				CalculateWaterCount(item);
			}
		}

		private void CalculateWaterCount(OrderItem item)
		{
			switch(item.Nomenclature.TareVolume)
			{
				case TareVolume.Vol19L:
					if(item.Nomenclature.IsDisposableTare)
					{
						DisposableWater19LCount += item.Count;
					}
					else
					{
						NotDisposableWater19LCount += item.Count;
					}
					break;
				case TareVolume.Vol6L:
					DisposableWater6LCount += item.Count;
					break;
				case TareVolume.Vol1500ml:
					DisposableWater1500mlCount += item.Count;
					break;
				case TareVolume.Vol600ml:
					DisposableWater600mlCount += item.Count;
					break;
				case TareVolume.Vol500ml:
					DisposableWater500mlCount += item.Count;
					break;
			}
		}

		private bool HasOrderEquipments(IUnitOfWork uow)
		{
			var allActiveFlyersNomenclaturesIds = new FlyerRepository().GetAllActiveFlyersNomenclaturesIdsByDate(uow, Order.DeliveryDate);
			
			if (!Order.ObservableOrderEquipments.Any() || OnlyFlyersInEquipments(allActiveFlyersNomenclaturesIds)) 
			{
				return false;
			}
			
			return true;
		}

		private bool OnlyFlyersInEquipments(IList<int> allActiveFlyersNomenclaturesIds)
		{
			foreach(OrderEquipment equipment in Order.ObservableOrderEquipments)
			{
				if(allActiveFlyersNomenclaturesIds.Contains(equipment.Nomenclature.Id))
				{
					continue;
				}

				return false;
			}

			return true;
		}

		public bool CompareWithDeliveryPriceRule(IDeliveryPriceRule rule)
		{
			decimal totalWater19LCount = DisposableWater19LCount + NotDisposableWater19LCount;
			bool deliveryIsFree = 
				(totalWater19LCount > 0 && totalWater19LCount >= rule.Water19LCount)
				|| (DisposableWater6LCount > 0 && DisposableWater6LCount >= rule.Water6LCount)
				|| (DisposableWater1500mlCount > 0 && DisposableWater1500mlCount >= rule.Water1500mlCount)
				|| (DisposableWater600mlCount > 0 && DisposableWater600mlCount >= rule.Water600mlCount)
				|| (DisposableWater500mlCount > 0 && DisposableWater500mlCount >= rule.Water500mlCount);

			return !deliveryIsFree;
		}
	}
}
