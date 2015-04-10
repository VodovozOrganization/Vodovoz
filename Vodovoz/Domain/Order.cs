using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Передвижения товаров")]
	public class Order: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string orderNumber;

		public virtual string OrderNumber {
			get { return orderNumber; }
			set { SetField (ref orderNumber, value, () => OrderNumber); }
		}
		//Строка или число?

		//TODO: Состояние. Enum?

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

		//TODO: Договор. Какой договор имеется в виду?

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

		//TODO: ID маршрутного листа

		bool selfDelivery;

		public virtual bool SelfDelivery {
			get { return selfDelivery; }
			set { SetField (ref selfDelivery, value, () => SelfDelivery); }
		}

		//TODO: Недовоз. ID предыдущего заказа. Что это за херь и с чем ее едят?

		int bottlesReturn;

		public virtual int BottlesReturn {
			get { return bottlesReturn; }
			set { SetField (ref bottlesReturn, value, () => BottlesReturn); }
		}
		//Что значит предположительное кол-во?

		string comment;

		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		//TODO: Подписание документов. Что тут должно быть? Enum?

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

		//TODO: Товары по заказу

		//TODO: Печатаемые документы

		//TODO: Оборудование по заказу

		//TODO: Сервисное обслуживание.

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion

		public Order ()
		{
			Comment = OrderNumber = String.Empty;
		}
	}
}

