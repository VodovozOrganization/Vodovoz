using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Маршрутные листы", ObjectName = "маршрутный лист")]
	public class RouteList: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField (ref driver, value, () => Driver); }
		}

		Car car;

		[Display (Name = "Машина")]
		public virtual Car Car {
			get { return car; }
			set { 
				SetField (ref car, value, () => Car); 
				if (value.Driver != null)
					Driver = value.Driver;
			}
		}

		DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		Decimal plannedDistance;

		[Display (Name = "Планируемое расстояние")]
		public virtual Decimal PlannedDistance {
			get { return plannedDistance; }
			set { SetField (ref plannedDistance, value, () => PlannedDistance); }
		}

		Decimal actualDistance;

		[Display (Name = "Фактическое расстояние")]
		public virtual Decimal ActualDistance {
			get { return actualDistance; }
			set { SetField (ref actualDistance, value, () => ActualDistance); }
		}

		RouteListStatus status;
		
		[Display (Name = "Статус")]
		public virtual RouteListStatus Status {
			get { return status; }
			set { SetField (ref status, value, () => Status); }
		}

		IList<Order> orders;

		[Display (Name = "Заказы")]
		public virtual IList<Order> Orders {
			get { return orders; }
			set { SetField (ref orders, value, () => Orders); }
		}

		GenericObservableList<Order> observableOrders;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<Order> ObservableOrders {
			get {
				if (observableOrders == null)
					observableOrders = new GenericObservableList<Order> (orders);
				return observableOrders;
			}
		}

		public virtual string Number { get { return Id.ToString (); } }

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Driver == null)
				yield return new ValidationResult ("Не заполнен водитель.");
			if (Car == null)
				yield return new ValidationResult ("На заполнен автомобиль.");
		}

		#endregion

		public RouteList ()
		{
			Date = DateTime.Now;
		}
	}

	public enum RouteListStatus
	{
		[ItemTitleAttribute ("Новый")] New,
		[ItemTitleAttribute ("Готов к отгрузке")] Ready,
		[ItemTitleAttribute ("На погрузке")]InLoading,
		[ItemTitleAttribute ("В пути")]EnRoute,
		[ItemTitleAttribute ("Не сдан")]NotDelivered,
		[ItemTitleAttribute ("Закрыт")]Closed
	}

	public class RouteListStatusStringType : NHibernate.Type.EnumStringType
	{
		public RouteListStatusStringType () : base (typeof(RouteListStatus))
		{
		}
	}
}

