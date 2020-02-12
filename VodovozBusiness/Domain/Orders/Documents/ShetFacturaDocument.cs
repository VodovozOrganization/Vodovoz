using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class ShetFacturaDocument:OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.ShetFactura;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = String.Format("Счет-Фактура {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.ShetFactura",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id }
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Счет-Фактура №{0}", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;
	}
}

