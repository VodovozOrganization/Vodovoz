using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using System;

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
			set { SetField (ref warehouse, value, () => Warehouse); }
		}

		//TODO Map invoice item to database

		IList<IncomingInvoiceItem> items;

		[Display (Name = "Строки")]
		public virtual IList<IncomingInvoiceItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
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
	}
}

