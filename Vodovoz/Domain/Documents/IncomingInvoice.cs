using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz
{

	[OrmSubject(JournalName = "Входящие накладные", ObjectName = "входящая накладная")]
	public class IncomingInvoice: Document
	{
		Counterparty contractor;
		[Display(Name = "Контрагент")]
		public virtual Counterparty Contractor {
			get { return contractor; }
			set { SetField (ref contractor, value, () => Contractor); }
		}

		Warehouse warehouse;
		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField (ref warehouse, value, () => Warehouse); }
		}

		//TODO Map invoice item to database

		IList<IncomingInvoiceItem> items;
		[Display(Name = "Строки")]
		public virtual IList<IncomingInvoiceItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

	}
}

