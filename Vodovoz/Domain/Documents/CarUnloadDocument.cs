using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Documents;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using System.Collections.Generic;
using Gamma.Utilities;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "документы разгрузки автомобилей",
		Nominative = "документ разгрузки автомобиля")]
	public class CarUnloadDocument:Document,IValidatableObject
	{
		public CarUnloadDocument ()
		{
		}

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

		IList<CarUnloadDocumentItem> items = new List<CarUnloadDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<CarUnloadDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<CarUnloadDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<CarUnloadDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<CarUnloadDocumentItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Разгрузка автомобиля №{0} от {1:d}", Id, TimeStamp); }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Документ разгрузки автомобиля"; }
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
			if (RouteList == null)
				yield return new ValidationResult ("Не указан маршрутный лист, по которому осуществляется разгрузка.",
					new[] { this.GetPropertyName (o => o.RouteList)});
		}

		#endregion

		public void AddItem (CarUnloadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add (item);
		}
	}
}


