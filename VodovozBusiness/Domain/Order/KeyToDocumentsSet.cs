using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using NHibernate.Util;
using System.Linq;

namespace Vodovoz.Domain.Orders
{
	public class KeyToDocumentsSet
	{
		public KeyToDocumentsSet(){}

		public KeyToDocumentsSet(Order order)
		{
			Order = order;
			InitializeFields();
		}

		[Display(Name = "Есть ли товары в заказе?")]
		public bool HasOrderItems { get; set; } = false;

		[Display(Name = "Есть ли оборудование в заказе?")]
		public bool HasOrderEquipment { get; set; } = false;

		[Display(Name = "Нужен ли возврат залога от клиента?")]
		public bool NeedToRefundDepositFromClient { get; set; } = false;

		[Display(Name = "Есть ли возврат тары?")]
		public bool? NeedToReturnBottles { get; set; } = null;

		[Display(Name = "Стоимость товаров заказа равна нулю?")]
		public bool IsPriceOfAllOrderItemsZero { get; set; } = false;

		[Display(Name = "Тип документа 'ТОРГ12 + Счёт-фактура'?")]
		private bool IsDocTypeTORG12 { get; set; } = false;

		[Display(Name = "Тип оплаты")]
		public PaymentType PaymentType { get; set; }

		[Display(Name = "Тип документа")]
		public DefaultDocumentType? DefaultDocumentType { get; set; } = Client.DefaultDocumentType.upd;

		[Display(Name = "Заказ, для которго создаётся набор документов")]
		private Order Order { get; set; }

		void InitializeFields()
		{
			this.DefaultDocumentType = Order.DocumentType ?? Order.Client.DefaultDocumentType;
			this.IsDocTypeTORG12 = DefaultDocumentType.HasValue && DefaultDocumentType == Client.DefaultDocumentType.torg12;
			this.HasOrderEquipment = Order.ObservableOrderEquipments.Any();
			this.HasOrderItems = Order.ObservableOrderItems.Any();
			this.IsPriceOfAllOrderItemsZero = Order.ObservableOrderItems.Sum(i => i.Price * i.Discount) <= 0m;
			this.NeedToReturnBottles = Order.BottlesReturn > 0;
			this.NeedToRefundDepositFromClient = Order.ObservableOrderDepositItems.Any(x => x.PaymentDirection == PaymentDirection.FromClient);
			this.PaymentType = Order.PaymentType;
		}
	}
}
