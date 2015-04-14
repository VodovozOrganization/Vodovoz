using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject ("Заказ")]
	public class Order: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		OrderStatus orderStatus;

		public virtual OrderStatus OrderStatus {
			get { return orderStatus; }
			set { SetField (ref orderStatus, value, () => OrderStatus); }
		}

		Counterparty client;

		public virtual Counterparty Client {
			get { return client; }
			set { SetField (ref client, value, () => Client); }
		}

		DeliveryPoint deliveryPoint;

		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField (ref deliveryPoint, value, () => DeliveryPoint); }
		}

		DateTime deliveryDate;

		public virtual DateTime DeliveryDate {
			get { return deliveryDate; }
			set { SetField (ref deliveryDate, value, () => DeliveryDate); }
		}

		DeliverySchedule deliverySchedule;

		public virtual DeliverySchedule DeliverySchedule {
			get { return deliverySchedule; }
			set { SetField (ref deliverySchedule, value, () => DeliverySchedule); }
		}

		bool selfDelivery;

		public virtual bool SelfDelivery {
			get { return selfDelivery; }
			set { SetField (ref selfDelivery, value, () => SelfDelivery); }
		}

		Order previousOrder;

		public virtual Order PreviousOrder {
			get { return previousOrder; }
			set { SetField (ref previousOrder, value, () => PreviousOrder); }
		}

		int bottlesReturn;

		public virtual int BottlesReturn {
			get { return bottlesReturn; }
			set { SetField (ref bottlesReturn, value, () => BottlesReturn); }
		}

		string comment;

		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		OrderSignatureType signatureType;

		public virtual OrderSignatureType SignatureType {
			get { return signatureType; }
			set { SetField (ref signatureType, value, () => SignatureType); }
		}

		Decimal sumToReceive;

		public virtual Decimal SumToReceive {
			get { return sumToReceive; }
			set { SetField (ref sumToReceive, value, () => SumToReceive); }
		}

		bool shipped;

		public virtual bool Shipped {
			get { return shipped; }
			set { SetField (ref shipped, value, () => Shipped); }
		}

		List<OrderItem> orderItems;

		public virtual List<OrderItem> OrderItems {
			get { return orderItems; }
			set { SetField (ref orderItems, value, () => OrderItems); }
		}

		List<OrderEquipment> orderEquipments;

		public virtual List<OrderEquipment> OrderEquipments {
			get { return orderEquipments; }
			set { SetField (ref orderEquipments, value, () => OrderEquipments); }
		}

		//TODO: ID маршрутного листа

		//TODO: Договор. Какой договор имеется в виду?

		//TODO: Печатаемые документы

		//TODO: Сервисное обслуживание.

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
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

