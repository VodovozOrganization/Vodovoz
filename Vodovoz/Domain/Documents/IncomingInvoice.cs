using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;

namespace Vodovoz.Domain.Documents
{

	[OrmSubject (JournalName = "Входящие накладные", ObjectName = "Входящая накладная")]
	public class IncomingInvoice: Document
	{
		Counterparty contractor;

		[Display (Name = "Контрагент")]
		public virtual Counterparty Contractor {
			get { return contractor; }
			set { SetField (ref contractor, value, () => Contractor); }
		}

		Warehouse warehouse;

		[Display (Name = "Склад")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField (ref warehouse, value, () => Warehouse); 
				foreach(var item in Items)
				{
					if (item.IncomeGoodsOperation.IncomingWarehouse != warehouse)
						item.IncomeGoodsOperation.IncomingWarehouse = warehouse;
				}
			}
		}

		//TODO Map invoice item to database

		IList<IncomingInvoiceItem> items = new List<IncomingInvoiceItem> ();

		[Display (Name = "Строки")]
		public virtual IList<IncomingInvoiceItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<IncomingInvoiceItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<IncomingInvoiceItem> ObservableItems {
			get {if (observableItems == null)
					observableItems = new GenericObservableList<IncomingInvoiceItem> (Items);
				return observableItems;
			}
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Входящая накладная"; }
		}

		new public virtual string Description {
			get { return String.Format ("Поставщик: {0}; Склад поступления: {1};", 
				Contractor != null ? Contractor.Name : "Не указан",
				Warehouse != null ? Warehouse.Name : "Не указан"); }
		}

		#endregion

		public void AddItem(IncomingInvoiceItem item)
		{
			item.IncomeGoodsOperation.IncomingWarehouse = warehouse;
			item.Document = this;
			ObservableItems.Add (item);
		}
	}
}

