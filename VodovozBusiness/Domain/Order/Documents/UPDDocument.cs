using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class UPDDocument:OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.UPD;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = String.Format("УПД {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.UPD",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "special", false }
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
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
			get	{
				return DocumentOrientation.Landscape;
			}
		}
	}
}

