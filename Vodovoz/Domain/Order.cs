using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Заказы", ObjectName = "заказ")]
	public class Order: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		OrderStatus orderStatus;

		[Display (Name = "Статус заказа")]
		public virtual OrderStatus OrderStatus {
			get { return orderStatus; }
			set { SetField (ref orderStatus, value, () => OrderStatus); }
		}

		Counterparty client;

		[Display (Name = "Клиент")]
		public virtual Counterparty Client {
			get { return client; }
			set { SetField (ref client, value, () => Client); }
		}

		DeliveryPoint deliveryPoint;

		[Display (Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField (ref deliveryPoint, value, () => DeliveryPoint); }
		}

		DateTime deliveryDate;

		[Display (Name = "Дата доставки")]
		public virtual DateTime DeliveryDate {
			get { return deliveryDate; }
			set { SetField (ref deliveryDate, value, () => DeliveryDate); }
		}

		DeliverySchedule deliverySchedule;

		[Display (Name = "Время доставки")]
		public virtual DeliverySchedule DeliverySchedule {
			get { return deliverySchedule; }
			set { SetField (ref deliverySchedule, value, () => DeliverySchedule); }
		}

		bool selfDelivery;

		[Display (Name = "Самовывоз")]
		public virtual bool SelfDelivery {
			get { return selfDelivery; }
			set { SetField (ref selfDelivery, value, () => SelfDelivery); }
		}

		Order previousOrder;

		[Display (Name = "Предыдущий заказ")]
		public virtual Order PreviousOrder {
			get { return previousOrder; }
			set { SetField (ref previousOrder, value, () => PreviousOrder); }
		}

		int bottlesReturn;

		[Display (Name = "Бутылей на возврат")]
		public virtual int BottlesReturn {
			get { return bottlesReturn; }
			set { SetField (ref bottlesReturn, value, () => BottlesReturn); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		OrderSignatureType signatureType;

		[Display (Name = "Подписание документов")]
		public virtual OrderSignatureType SignatureType {
			get { return signatureType; }
			set { SetField (ref signatureType, value, () => SignatureType); }
		}

		Decimal sumToReceive;

		[Display (Name = "Сумма к получению")]
		public virtual Decimal SumToReceive {
			get { return sumToReceive; }
			set { SetField (ref sumToReceive, value, () => SumToReceive); }
		}

		bool shipped;

		[Display (Name = "Отгружено по платежке")]
		public virtual bool Shipped {
			get { return shipped; }
			set { SetField (ref shipped, value, () => Shipped); }
		}

		IList<OrderItem> orderItems = new List<OrderItem> ();

		[Display (Name = "Строки заказа")]
		public virtual IList<OrderItem> OrderItems {
			get { return orderItems; }
			set { SetField (ref orderItems, value, () => OrderItems); }
		}

		GenericObservableList<OrderItem> observableOrderItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<OrderItem> ObservableOrderItems {
			get {
				if (observableOrderItems == null)
					observableOrderItems = new GenericObservableList<OrderItem> (orderItems);
				return observableOrderItems;
			}
		}

		IList<OrderEquipment> orderEquipments = new List<OrderEquipment> ();

		[Display (Name = "Список оборудования")]
		public virtual IList<OrderEquipment> OrderEquipments {
			get { return orderEquipments; }
			set { SetField (ref orderEquipments, value, () => OrderEquipments); }
		}

		GenericObservableList<OrderEquipment> observableOrderEquipments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<OrderEquipment> ObservableOrderEquipments {
			get {
				if (observableOrderEquipments == null)
					observableOrderEquipments = new GenericObservableList<OrderEquipment> (orderEquipments);
				return observableOrderEquipments;
			}
		}

		RouteList routeList;

		[Display (Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get { return routeList; }
			set { SetField (ref routeList, value, () => RouteList); }
		}

		//TODO: Договор. Какой договор имеется в виду?

		//TODO: Печатаемые документы

		//TODO: Сервисное обслуживание.

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (DeliveryPoint == null)
				yield return new ValidationResult ("Необходимо заполнить точку доставки.",
					new[] { this.GetPropertyName (o => o.DeliveryPoint) });
			if (Client == null)
				yield return new ValidationResult ("Необходимо заполнить поле \"клиент\".",
					new[] { this.GetPropertyName (o => o.Client) });
		}

		#endregion

		public Order ()
		{
			Comment = String.Empty;
			OrderStatus = OrderStatus.NewOrder;
		}
	}

	public enum OrderStatus
	{
		[ItemTitleAttribute ("Новый")] NewOrder,
		[ItemTitleAttribute ("Принят")] Accepted,
		[ItemTitleAttribute ("В маршрутном листе")]InTravelList,
		[ItemTitleAttribute ("На погрузке")]OnLoading,
		[ItemTitleAttribute ("В пути")]OnTheWay,
		[ItemTitleAttribute ("Доставлен")]Shipped,
		[ItemTitleAttribute ("Выгрузка на складе")]UnloadingOnStock,
		[ItemTitleAttribute ("Отчет не закрыт")]ReportNotClosed,
		[ItemTitleAttribute ("Закрыт")]Closed,
		[ItemTitleAttribute ("Отменен")]Canceled,
		[ItemTitleAttribute ("Недовоз")]NotDelivered
	}

	public class OrderStatusStringType : NHibernate.Type.EnumStringType
	{
		public OrderStatusStringType () : base (typeof(OrderStatus))
		{
		}
	}

	public enum OrderSignatureType
	{
		[ItemTitleAttribute ("По печати")]BySeal,
		[ItemTitleAttribute ("По доверенности")]ByProxy
	}

	public class OrderSignatureTypeStringType : NHibernate.Type.EnumStringType
	{
		public OrderSignatureTypeStringType () : base (typeof(OrderSignatureType))
		{
		}
	}
}

