using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate.Util;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders
{
	public class OrderStateKey
	{
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

		[Display(Name = "Статус заказа")]
		public OrderStatus OrderStatus { get; set; }

		[Display(Name = "Тип оплаты")]
		public PaymentType PaymentType { get; set; }

		[Display(Name = "Есть ли специальные поля для печати в контрагенте?")]
		public bool HaveSpecialFields { get; set; }

		[Display(Name = "Тип документа")]
		public DefaultDocumentType? DefaultDocumentType { get; set; } = Client.DefaultDocumentType.upd;

		[Display(Name = "Заказ, для которго создаётся набор документов")]
		public Order Order { get; set; }

		public IEnumerable<OrderEquipment> OnlyEquipments => Order.OrderEquipments.Where(x => x.Nomenclature.Category == Goods.NomenclatureCategory.equipment);

		void InitializeFields()
		{
			this.DefaultDocumentType = Order.DocumentType ?? Order.Client.DefaultDocumentType;
			this.IsDocTypeTORG12 = DefaultDocumentType.HasValue && DefaultDocumentType == Client.DefaultDocumentType.torg12;
			this.HasOrderEquipment = Order.ObservableOrderEquipments.Any();
			this.HasOrderItems = Order.ObservableOrderItems.Any();
			this.IsPriceOfAllOrderItemsZero = Order.ObservableOrderItems.Sum(i => i.Sum) <= 0m;
			this.NeedToReturnBottles = Order.BottlesReturn > 0;
			this.NeedToRefundDepositToClient = Order.ObservableOrderDepositItems.Any(x => x.PaymentDirection == PaymentDirection.ToClient);
			this.PaymentType = Order.PaymentType;
			this.HaveSpecialFields = Order.Client.UseSpecialDocFields;
			this.NeedMaster = Order.OrderItems.Any(i => i.Nomenclature.Category == Goods.NomenclatureCategory.master);
			this.IsSelfDelivery = Order.SelfDelivery;
		}
	}
}
