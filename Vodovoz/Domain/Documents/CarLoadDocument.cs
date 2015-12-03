using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "документы погрузки автомобилей",
		Nominative = "документ погрузки автомобиля")]
	public class CarLoadDocument: Document, IValidatableObject
	{
		Employee storekeeper;

		[Required (ErrorMessage = "Должен быть указан кладовщик.")]
		[Display (Name = "Кладовщик")]
		public virtual Employee Storekeeper {
			get { return storekeeper; }
			set { SetField (ref storekeeper, value, () => Storekeeper); }
		}

		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.MovementOperation.OperationTime != TimeStamp)
						item.MovementOperation.OperationTime = TimeStamp;
				}
			}
		}

		Order order;

		public virtual Order Order {
			get { return order; } 
			set { SetField (ref order, value, () => Order); }
		}

		RouteList routeList;

		public virtual RouteList RouteList {
			get { return routeList; }
			set { SetField (ref routeList, value, () => RouteList); }
		}

		Warehouse warehouse;

		public virtual Warehouse Warehouse {
			get { return Warehouse; }
			set { SetField (ref warehouse, value, () => Warehouse); }
		}


		IList<CarLoadDocumentItem> items = new List<CarLoadDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<CarLoadDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<CarLoadDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<CarLoadDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<CarLoadDocumentItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Погрузка автомобиля №{0} от {1:d}", Id, TimeStamp); }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Документ погрузки автомобиля"; }
		}

		new public virtual string Description {
			get { 
				return "";
			}
		}

		#endregion

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Storekeeper == null)
				yield return new ValidationResult ("Не указан кладовщик.",
					new[] { this.GetPropertyName (o => o.Storekeeper) });
			if (RouteList == null && Order == null)
				yield return new ValidationResult ("Не указаны ни заказ, ни маршрутный лист, по которым осуществляется отгрузка.",
					new[] { this.GetPropertyName (o => o.RouteList), this.GetPropertyName (o => o.Order) });
		}

		#endregion

		public void AddItem (CarLoadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add (item);
		}
	}
}

