using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
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
			this.OrderStatus = Order.OrderStatus;
			InitializeFields();
		}

		/// <summary>
		/// Создает ключ для определенного требуемого статуса
		/// </summary>
		public OrderStateKey(Order order, OrderStatus requiredStatus)
		{
			Order = order;
			this.OrderStatus = requiredStatus;
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
		public decimal Water19LCount { get; set; }

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
			this.DefaultDocumentType = Order.DocumentType ?? Order.Client.DefaultDocumentType;
			this.IsDocTypeTORG12 = DefaultDocumentType.HasValue && DefaultDocumentType == Domain.Client.DefaultDocumentType.torg12;

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
			
			this.IsPriceOfAllOrderItemsZero = Order.ObservableOrderItems.Sum(i => i.ActualSum) <= 0m;
			this.NeedToReturnBottles = Order.BottlesReturn > 0;
			this.NeedToRefundDepositToClient = Order.ObservableOrderDepositItems.Any();
			this.PaymentType = Order.PaymentType;
			this.HaveSpecialFields = Order.Client.UseSpecialDocFields;
			this.NeedMaster = Order.OrderItems.Any(i => i.Nomenclature.Category == Domain.Goods.NomenclatureCategory.master);
			this.IsSelfDelivery = Order.SelfDelivery;
			this.PayAfterShipment = Order.PayAfterShipment;
			this.HasEShopOrder = Order.EShopOrder.HasValue;

			//для проверки цены доставки
			this.Water19LCount = Order.OrderItems
				.Where(x => x.Nomenclature != null && x.Nomenclature.IsWater19L)
				.Sum(x => x.Count);
			this.DisposableWater19LCount = Order.OrderItems
				.Where(x => x.Nomenclature != null
				&& x.Nomenclature.Category == NomenclatureCategory.water
				&& x.Nomenclature.IsDisposableTare
				&& x.Nomenclature.TareVolume == TareVolume.Vol19L)
				.Sum(x => x.Count);
			this.DisposableWater6LCount = Order.OrderItems
				.Where(x => x.Nomenclature != null
				&& x.Nomenclature.Category == NomenclatureCategory.water
				&& x.Nomenclature.IsDisposableTare
				&& x.Nomenclature.TareVolume == TareVolume.Vol6L)
				.Sum(x => x.Count);
			this.DisposableWater1500mlCount = Order.OrderItems
				.Where(x => x.Nomenclature != null
				&& x.Nomenclature.Category == NomenclatureCategory.water
				&& x.Nomenclature.IsDisposableTare
				&& x.Nomenclature.TareVolume == TareVolume.Vol1500ml)
				.Sum(x => x.Count);
			this.DisposableWater600mlCount = Order.OrderItems
				.Where(x => x.Nomenclature != null
				&& x.Nomenclature.Category == NomenclatureCategory.water
				&& x.Nomenclature.IsDisposableTare
				&& x.Nomenclature.TareVolume == TareVolume.Vol600ml)
				.Sum(x => x.Count);
			this.DisposableWater500mlCount = Order.OrderItems
				.Where(x => x.Nomenclature != null
	            && x.Nomenclature.Category == NomenclatureCategory.water
	            && x.Nomenclature.IsDisposableTare
	            && x.Nomenclature.TareVolume == TareVolume.Vol500ml)
				.Sum(x => x.Count);
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
			var total19LWater = Water19LCount + DisposableWater19LCount;
			decimal totalNo19LWater = DisposableWater6LCount / rule.Water6LCount;
			totalNo19LWater += DisposableWater1500mlCount / rule.Water1500mlCount;
			totalNo19LWater += DisposableWater600mlCount / rule.Water600mlCount;
			totalNo19LWater += DisposableWater500mlCount / rule.Water500mlCount;
			total19LWater += (int)totalNo19LWater;

			bool result = total19LWater < rule.Water19LCount;

			return result;
		}
	}
}
