using System;
using System.Collections.Generic;
using QS.Print;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	public class UPDDocument:OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument

		public virtual QSReport.ReportInfo GetReportInfo ()
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

		public override DateTime? DocumentDate {
			get { return Order?.DeliveryDate; }
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

