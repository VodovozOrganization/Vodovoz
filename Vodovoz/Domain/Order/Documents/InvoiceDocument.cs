using System;
using System.Collections.Generic;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceDocument:OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Накладная №{0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = Order.PaymentType==PaymentType.barter ? "Documents.InvoiceBarter" : "Documents.Invoice",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id }
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.Invoice;
			}
		}

		#endregion

		public override string Name { get { return String.Format ("Накладная №{0}",Order.Id); } }

		public override string DocumentDate {
			get { return String.Format ("от {0}", Order.DeliveryDate.ToShortDateString ()); }
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}

	}
}

