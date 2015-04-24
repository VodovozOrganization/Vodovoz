using System;
using QSOrmProject;
using System.Collections.Generic;

namespace Vodovoz
{
	public class IncomingInvoice: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		DateTime date;

		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

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

		IList<IncomingInvoiceItem> items;

		public virtual IList<IncomingInvoiceItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

	}
}

