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
				Identifier = "Documents.UPD",
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

		public override string Name { get { return String.Format ("УПД №{0}", Order.Id); } }

		public override string DocumentDate {
			get { return String.Format ("от {0}", Order.DeliveryDate.ToShortDateString ()); }
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}

		public override DocumentOrientation Orientation
		{
			get
			{
				return DocumentOrientation.Landscape;
			}
		}
	}
}

