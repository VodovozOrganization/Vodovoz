using System;
using System.Collections.Generic;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceBarterDocument : OrderDocument
	{
		#region implemented abstract members of OrderDocument
		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Накладная №{0} от {1:d} (безденежно)", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.InvoiceBarter",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id }
				}
			};
		}
		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.InvoiceBarter;
			}
		}
		#endregion

		public override string Name { get { return String.Format ("Накладная №{0} (бартер)",Order.Id); }  }

		public override DateTime? DocumentDate {
			get { return Order?.DeliveryDate; }
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}
	}
}

