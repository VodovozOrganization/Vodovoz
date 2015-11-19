using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "маршрутные листы",
		Nominative = "маршрутный лист")]
	public class RouteList: BusinessObjectBase<RouteList>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField (ref driver, value, () => Driver); }
		}

		Employee forwarder;

		[Display (Name = "Экспедитор")]
		public virtual Employee Forwarder {
			get { return forwarder; }
			set { SetField (ref forwarder, value, () => Forwarder); }
		}

		Employee logistican;

		[Display (Name = "Логист")]
		public virtual Employee Logistican {
			get { return logistican; }
			set { SetField (ref logistican, value, () => Logistican); }
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

		DeliveryShift shift;

		[Display (Name = "Смена доставки")]
		public virtual DeliveryShift Shift {
			get { return shift; }
			set { 
				SetField (ref shift, value, () => Shift); 
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

		IList<RouteListItem> addresses = new List<RouteListItem> ();

		[Display (Name = "Адреса в маршрутном листе")]
		public virtual IList<RouteListItem> Addresses {
			get { return addresses; }
			set { 
				SetField (ref addresses, value, () => Addresses); 
				SetNullToObservableAddresses ();
			}
		}

		GenericObservableList<RouteListItem> observableAddresses;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<RouteListItem> ObservableAddresses {
			get {
				if (observableAddresses == null) {
					observableAddresses = new GenericObservableList<RouteListItem> (addresses);
					observableAddresses.ElementAdded += ObservableAddresses_ElementAdded;
					observableAddresses.ElementRemoved += ObservableAddresses_ElementRemoved;
				}
				return observableAddresses;
			}
		}

		void ObservableAddresses_ElementRemoved (object aList, int[] aIdx, object aObject)
		{
			CheckAddressOrder ();
		}

		void ObservableAddresses_ElementAdded (object aList, int[] aIdx)
		{
			CheckAddressOrder ();
		}

		public virtual string DateString { get { return Date.ToShortDateString (); } }

		public virtual string StatusString { get { return Status.GetEnumTitle (); } }

		public virtual string DriverInfo { get { return String.Format ("{0} - {1}", Driver.FullName, Car.Title); } }

		public virtual string Title { get { return String.Format ("Маршрутный лист №{0}", Id); } }

		public RouteListItem AddAddressFromOrder (Order order)
		{
			if (order.DeliveryPoint == null)
				throw new NullReferenceException ("В маршрутный нельзя добавить заказ без точки доставки.");
			var item = new RouteListItem (this, order);
			ObservableAddresses.Add (item);
			return item;
		}

		public void RemoveAddress (RouteListItem address)
		{
			address.RemovedFromRoute ();
			UoW.Delete (address);
			ObservableAddresses.Remove (address);
		}

		private void CheckAddressOrder ()
		{
			int i = 0;
			foreach (var address in Addresses) {
				if (address.IndexInRoute != i)
					address.IndexInRoute = i;
				i++;
			}
		}

		private void SetNullToObservableAddresses ()
		{
			if (observableAddresses == null)
				return;
			observableAddresses.ElementAdded -= ObservableAddresses_ElementAdded;
			observableAddresses.ElementRemoved -= ObservableAddresses_ElementRemoved;
			observableAddresses = null;
		}

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (validationContext.Items.ContainsKey ("NewStatus")) {
				RouteListStatus newStatus = (RouteListStatus)validationContext.Items ["NewStatus"];
				if (newStatus == RouteListStatus.Ready) {
				}
			}

			if (Shift == null)
				yield return new ValidationResult ("Смена маршрутного листа должна быть заполнена.",
					new[] { this.GetPropertyName (o => o.Shift) });

			if (Driver == null)
				yield return new ValidationResult ("Не заполнен водитель.",
					new[] { this.GetPropertyName (o => o.Driver) });
			if (Car == null)
				yield return new ValidationResult ("На заполнен автомобиль.",
					new[] { this.GetPropertyName (o => o.Car) });
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

