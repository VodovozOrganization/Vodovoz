using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Gamma.Utilities;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "документы погрузки автомобилей",
		Nominative = "документ погрузки автомобиля")]
	public class CarLoadDocument: Document, IValidatableObject
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				if (!NHibernate.NHibernateUtil.IsInitialized(Items))
					return;
				foreach (var item in Items) {
					if (item.MovementOperation != null && item.MovementOperation.OperationTime != TimeStamp)
						item.MovementOperation.OperationTime = TimeStamp;
				}
			}
		}
			
		RouteList routeList;

		public virtual RouteList RouteList {
			get { return routeList; }
			set { SetField (ref routeList, value, () => RouteList); }
		}

		Warehouse warehouse;

		public virtual Warehouse Warehouse {
			get { return warehouse; }
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
		public virtual GenericObservableList<CarLoadDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<CarLoadDocumentItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Талон погрузки №{0} от {1:d}", Id, TimeStamp); }
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Author == null)
				yield return new ValidationResult ("Не указан кладовщик.",
					new[] { this.GetPropertyName (o => o.Author) });
			if (RouteList == null)
				yield return new ValidationResult ("Не указан маршрутный лист, по которым осуществляется отгрузка.",
					new[] { this.GetPropertyName (o => o.RouteList)});
		}

		#endregion

		public virtual void AddItem (CarLoadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add (item);
		}
	}
}

