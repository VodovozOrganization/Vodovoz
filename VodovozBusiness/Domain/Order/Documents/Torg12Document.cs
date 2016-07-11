using System;
using System.Collections.Generic;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	public class Torg12Document:OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("ТОРГ-12 {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.Torg12",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id }
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.Torg12;
			}
		}

		#endregion

		public override string Name { get { return String.Format ("ТОРГ-12 №{0}", Order.Id); } }

		public override string DocumentDate {
			get { return String.Format ("от {0:d}", Order.DeliveryDate); }
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

