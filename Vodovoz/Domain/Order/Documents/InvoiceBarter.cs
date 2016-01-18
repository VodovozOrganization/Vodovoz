using System;
using System.Collections.Generic;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceBarter:OrderDocument
	{
		#region implemented abstract members of OrderDocument
		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Накладная №{0} от {1:d} (безденежно)", Order.Id, Order.DeliveryDate),
				Identifier = "InvoiceBarter",
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

		public override string Name { get { return "Накладная (безденежно)"; } }
	}
}

