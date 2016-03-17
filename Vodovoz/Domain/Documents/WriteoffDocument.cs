using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QSOrmProject;
using Vodovoz.Domain.Store;
using Gamma.Utilities;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "акты списания ТМЦ",
		Nominative = "акт списания ТМЦ")]
	public class WriteoffDocument: Document, IValidatableObject
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.WarehouseWriteoffOperation != null && item.WarehouseWriteoffOperation.OperationTime != TimeStamp)
						item.WarehouseWriteoffOperation.OperationTime = TimeStamp;
					if (item.CounterpartyWriteoffOperation != null && item.CounterpartyWriteoffOperation.OperationTime != TimeStamp)
						item.CounterpartyWriteoffOperation.OperationTime = TimeStamp;			
				}
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		Employee responsibleEmployee;

		[Required (ErrorMessage = "Должен быть указан ответственнй за списание.")]
		[Display (Name = "Ответственный")]
		public virtual Employee ResponsibleEmployee {
			get { return responsibleEmployee; }
			set { responsibleEmployee = value; }
		}

		Counterparty client;

		[Display (Name = "Клиент списания")]
		public virtual Counterparty Client {
			get { return client; }
			set {
				client = value;
				if (Client != null)
					WriteoffWarehouse = null;
				if (Client == null || !Client.DeliveryPoints.Contains (DeliveryPoint))
					DeliveryPoint = null;
				foreach (var item in Items) {
					if (item.CounterpartyWriteoffOperation != null && item.CounterpartyWriteoffOperation.WriteoffCounterparty != client)
						item.CounterpartyWriteoffOperation.WriteoffCounterparty = client;
				}
			}
		}

		DeliveryPoint deliveryPoint;

		[Display (Name = "Точка доставки списания")]
		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { 
				deliveryPoint = value; 
				foreach (var item in Items) {
					if (item.CounterpartyWriteoffOperation != null && item.CounterpartyWriteoffOperation.WriteoffDeliveryPoint != deliveryPoint)
						item.CounterpartyWriteoffOperation.WriteoffDeliveryPoint = deliveryPoint;
				}
			}
		}

		Warehouse writeoffWarehouse;

		[Display (Name = "Склад списания")]
		public virtual Warehouse WriteoffWarehouse {
			get { return writeoffWarehouse; }
			set {
				writeoffWarehouse = value;
				if (WriteoffWarehouse != null)
					Client = null;
				
				foreach (var item in Items) {
					if (item.WarehouseWriteoffOperation != null && item.WarehouseWriteoffOperation.WriteoffWarehouse != writeoffWarehouse)
						item.WarehouseWriteoffOperation.WriteoffWarehouse = writeoffWarehouse;
				}
			}
		}

		IList<WriteoffDocumentItem> items = new List<WriteoffDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<WriteoffDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<WriteoffDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WriteoffDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<WriteoffDocumentItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Акт списания №{0} от {1:d}", Id, TimeStamp); }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Акт списания"; }
		}

		new public virtual string Description {
			get { 
				if (WriteoffWarehouse != null)
					return String.Format ("Со склада \"{0}\"", WriteoffWarehouse.Name);
				else if (Client != null)
					return String.Format ("От клиента \"{0}\"", Client.Name);
				return "";
			}
		}

		#endregion

		public virtual void AddItem (Nomenclature nomenclature, decimal amount, decimal inStock)
		{
			var item = new WriteoffDocumentItem
			{ 
				Nomenclature = nomenclature,
				AmountOnStock = inStock,
				Amount = amount,
				Document = this
			};
			if (WriteoffWarehouse != null)
				item.CreateOperation(WriteoffWarehouse, TimeStamp);
			else
				item.CreateOperation(Client, DeliveryPoint, TimeStamp);
			ObservableItems.Add (item);
		}

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (WriteoffWarehouse == null && Client == null)
				yield return new ValidationResult ("Склад списания или контрагент должны быть заполнены.");
			if (Client != null && DeliveryPoint == null)
				yield return new ValidationResult ("Точка доставки должна быть указана.");

			if(Items.Any(i => i.Amount <= 0))
				yield return new ValidationResult ("В списке списания присутствуют позиции с нулевым количеством.",
					new[] { this.GetPropertyName (o => o.Items) });
		}

		public WriteoffDocument ()
		{
			Comment = String.Empty;
		}

	}

	public enum WriteoffType
	{
		[ItemTitleAttribute ("От клиента")]
		counterparty,
		[ItemTitleAttribute ("Со склада")]
		warehouse
	}

	public class WriteoffStringType : NHibernate.Type.EnumStringType
	{
		public WriteoffStringType () : base (typeof(WriteoffType))
		{
		}
	}
}

