using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate.Util;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders
{
	public class KeyToDocumentsSet
	{
		public KeyToDocumentsSet() { }

		public KeyToDocumentsSet(Order order)
		{
			Order = order;
			this.OrderStatus = Order.OrderStatus;
			InitializeFields();
		}

		/// <summary>
		/// Создает ключ для определенного требуемого статуса
		/// </summary>
		public KeyToDocumentsSet(Order order, OrderStatus requiredStatus)
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
			this.IsPriceOfAllOrderItemsZero = Order.ObservableOrderItems.Sum(i => i.Price * (1 - (decimal)i.Discount / 100)) <= 0m;
			this.NeedToReturnBottles = Order.BottlesReturn > 0;
			this.NeedToRefundDepositToClient = Order.ObservableOrderDepositItems.Any(x => x.PaymentDirection == PaymentDirection.ToClient);
			this.PaymentType = Order.PaymentType;
			this.NeedMaster = Order.OrderItems.Any(i => i.Nomenclature.Category == Goods.NomenclatureCategory.master);
			this.IsSelfDelivery = Order.SelfDelivery;
		}

		#region после добавления любого свойства или поля, которые учавствуют в формировании ключа для нового правила, обязательно добавить эти поля в переопределение методов Equals, ==, !=, GetHashCode
		/*
		public override bool Equals(object obj)
		{
			if(obj == null || this.GetType() != obj.GetType())
				return false;

			KeyToDocumentsSet set = (KeyToDocumentsSet)obj;
			bool result = this.HasOrderItems == set.HasOrderItems
							  && this.HasOrderEquipment == set.HasOrderEquipment
							  && this.NeedToRefundDepositFromClient == set.NeedToRefundDepositFromClient
							  && this.NeedToReturnBottles == set.NeedToReturnBottles
							  && this.IsPriceOfAllOrderItemsZero == set.IsPriceOfAllOrderItemsZero
							  && this.PaymentType == set.PaymentType
							  && this.DefaultDocumentType == set.DefaultDocumentType;
			return result;
		}

		public static bool operator ==(KeyToDocumentsSet x, KeyToDocumentsSet y)
		{
			bool result = x.HasOrderItems == y.HasOrderItems
						   && x.HasOrderEquipment == y.HasOrderEquipment
						   && x.NeedToRefundDepositFromClient == y.NeedToRefundDepositFromClient
						   && x.NeedToReturnBottles == y.NeedToReturnBottles
						   && x.IsPriceOfAllOrderItemsZero == y.IsPriceOfAllOrderItemsZero
						   && x.PaymentType == y.PaymentType
						   && x.DefaultDocumentType == y.DefaultDocumentType;
			return result;
		}

		public static bool operator !=(KeyToDocumentsSet x, KeyToDocumentsSet y)
		{
			return !(x == y);
		}

		public override int GetHashCode()
		{
			int result = 0;
			result += 31 * result + this.HasOrderItems.GetHashCode();
			result += 31 * result + this.HasOrderEquipment.GetHashCode();
			result += 31 * result + this.NeedToRefundDepositFromClient.GetHashCode();
			result += 31 * result + this.NeedToReturnBottles.GetHashCode();
			result += 31 * result + this.IsPriceOfAllOrderItemsZero.GetHashCode();
			result += 31 * result + this.PaymentType.GetHashCode();
			result += 31 * result + this.DefaultDocumentType.GetHashCode();
			return result;
		}*/
		#endregion
	}
}
