using System;
using System.Collections.Generic;

namespace Vodovoz.Domain.Orders.Documents
{
	public class UPDDocument:OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("УПД {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "UPD",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id }
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.UPD;
			}
		}

		#endregion

		public override string Name { get { return "УПД"; } }

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}
	}
}

