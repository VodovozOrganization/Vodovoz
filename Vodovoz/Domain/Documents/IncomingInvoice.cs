using System.Collections.Generic;

namespace Vodovoz
{
	public class IncomingInvoice: Document
	{
		Counterparty contractor;

		public virtual Counterparty Contractor {
			get { return contractor; }
			set { SetField (ref contractor, value, () => Contractor); }
		}

		Warehouse warehouse;

		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField (ref warehouse, value, () => Warehouse); }
		}

		//TODO Map invoice item to database

		IList<IncomingInvoiceItem> items;

		public virtual IList<IncomingInvoiceItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

	}
}

