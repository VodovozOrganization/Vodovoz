using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using System.Data.Bindings;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (JournalName = "Списания ТМЦ", ObjectName = "Акт списания")]
	public class WriteoffDocument: Document
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.WriteOffGoodsOperation.OperationTime != TimeStamp)
						item.WriteOffGoodsOperation.OperationTime = TimeStamp;
				}
			}
		}

		Employee responsibleEmployee;

		[Required (ErrorMessage = "Должен быть указан ответственнй за перемещение.")]
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
				foreach (var item in Items) {
					if (item.WriteOffGoodsOperation.WriteoffCounterparty != client)
						item.WriteOffGoodsOperation.WriteoffCounterparty = client;
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
					if (item.WriteOffGoodsOperation.WriteoffDeliveryPoint != deliveryPoint)
						item.WriteOffGoodsOperation.WriteoffDeliveryPoint = deliveryPoint;
				}
			}
		}

		Warehouse writeoffWarehouse;

		[Display (Name = "Склад списания")]
		public virtual Warehouse WriteoffWarehouse {
			get { return writeoffWarehouse; }
			set {
				writeoffWarehouse = value; 
				foreach (var item in Items) {
					if (item.WriteOffGoodsOperation.WriteoffWarehouse != writeoffWarehouse)
						item.WriteOffGoodsOperation.WriteoffWarehouse = writeoffWarehouse;
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
		public GenericObservableList<WriteoffDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<WriteoffDocumentItem> (Items);
				return observableItems;
			}
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

		public void AddItem (WriteoffDocumentItem item)
		{
			item.WriteOffGoodsOperation.WriteoffWarehouse = WriteoffWarehouse;
			item.WriteOffGoodsOperation.WriteoffCounterparty = Client;
			item.WriteOffGoodsOperation.WriteoffDeliveryPoint = DeliveryPoint;
			item.WriteOffGoodsOperation.OperationTime = TimeStamp;
			item.Document = this;
			ObservableItems.Add (item);
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

