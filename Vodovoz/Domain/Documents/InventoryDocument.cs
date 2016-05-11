using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "инвентаризации",
		Nominative = "инвентаризация")]
	public class InventoryDocument: Document, IValidatableObject
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.WarehouseChangeOperation != null && item.WarehouseChangeOperation.OperationTime != TimeStamp)
						item.WarehouseChangeOperation.OperationTime = TimeStamp;
				}
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		Warehouse warehouse;

		[Display (Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set {
				warehouse = value;

				foreach (var item in Items) {
					if (item.WarehouseChangeOperation != null && item.WarehouseChangeOperation.WriteoffWarehouse != Warehouse)
						item.WarehouseChangeOperation.WriteoffWarehouse = Warehouse;
				}
			}
		}

		IList<InventoryDocumentItem> items = new List<InventoryDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<InventoryDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<InventoryDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<InventoryDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<InventoryDocumentItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Инвентаризация №{0} от {1:d}", Id, TimeStamp); }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Инвентаризация"; }
		}

		new public virtual string Description {
			get { 
				return "";
			}
		}

		#endregion

		public virtual void AddItem (Nomenclature nomenclature, decimal amount, decimal inStock)
		{
			var item = new InventoryDocumentItem()
			{ 
				Nomenclature = nomenclature,
				//AmountOnStock = inStock,
				AmountInDB = amount,
				Document = this
			};
			if (Warehouse != null)
				item.CreateOperation(Warehouse, TimeStamp);
			ObservableItems.Add (item);
		}

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Items.Any(i => i.AmountInDB <= 0))
				yield return new ValidationResult ("В списке списания присутствуют позиции с нулевым количеством.",
					new[] { this.GetPropertyName (o => o.Items) });
		}

		public InventoryDocument ()
		{
		}

	}
}

