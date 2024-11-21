using Autofac;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Tools.Orders
{
	public class OrderStateKey : ComparerDeliveryPrice
	{
		private readonly IOrderSettings _orderSettings;

		[Display(Name = "Заказ, для которого определяются параметры")]
		public Order Order { get; private set; }

		public OrderStateKey(IOrderSettings orderSettings)
		{
			_orderSettings = orderSettings ?? throw new System.ArgumentNullException(nameof(orderSettings));
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

		[Display(Name = "Самовывоз?")] public bool IsSelfDelivery { get; set; } = false;

		[Display(Name = "Оплата после отгрузки")]
		public bool PayAfterShipment { get; internal set; }

		[Display(Name = "Статус заказа")] public OrderStatus OrderStatus { get; set; }

		[Display(Name = "Тип оплаты")] public PaymentType PaymentType { get; set; }

		[Display(Name = "Есть ли специальные поля для печати в контрагенте?")]
		public bool HaveSpecialFields { get; set; }

		[Display(Name = "Тип документа")]
		public DefaultDocumentType? DefaultDocumentType { get; set; } = Domain.Client.DefaultDocumentType.upd;

		[Display(Name = "Заказ интернет магазина ?")]
		public bool HasEShopOrder { get; set; } = false;

		#endregion

		public IEnumerable<OrderEquipment> OnlyEquipments =>
			Order.OrderEquipments.Where(x => x.Nomenclature.Category == NomenclatureCategory.equipment);

		public override void InitializeFields(Order order, OrderStatus? requiredStatus = null)
		{
			Order = order;
			DeliveryDate = order.DeliveryDate;
			OrderStatus = requiredStatus ?? Order.OrderStatus;

			var nomenclatureSettings = ScopeProvider.Scope.Resolve<INomenclatureSettings>();
			//для документов
			DefaultDocumentType = Order.DocumentType ?? Order.Client.DefaultDocumentType;
			IsDocTypeTORG12 = DefaultDocumentType.HasValue && DefaultDocumentType == Domain.Client.DefaultDocumentType.torg12;

			HasOrderEquipment = HasOrderEquipments(Order.UoW);

			if(!Order.ObservableOrderItems.Any() ||
				(Order.ObservableOrderItems.Count == 1 && Order.ObservableOrderItems.Any(x =>
					x.Nomenclature.Id == nomenclatureSettings.PaidDeliveryNomenclatureId)))
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
			NeedMaster = Order.OrderItems.Any(i => i.Nomenclature.Category == NomenclatureCategory.master);
			IsSelfDelivery = Order.SelfDelivery;
			PayAfterShipment = Order.PayAfterShipment;
			HasEShopOrder = Order.EShopOrder.HasValue;

			//для проверки цены доставки
			CalculateAllWaterCount(Order.OrderItems);
		}

		private bool HasOrderEquipments(IUnitOfWork uow)
		{
			var allActiveFlyersNomenclaturesIds = ScopeProvider.Scope.Resolve<IFlyerRepository>().GetAllActiveFlyersNomenclaturesIdsByDate(uow, Order.DeliveryDate);

			if(!Order.ObservableOrderEquipments.Any() || OnlyFlyersInEquipments(allActiveFlyersNomenclaturesIds))
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
	}
}
