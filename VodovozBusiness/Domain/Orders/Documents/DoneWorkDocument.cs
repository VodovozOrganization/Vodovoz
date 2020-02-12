using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class DoneWorkDocument:OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.DoneWorkReport;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = Name,
				Identifier = "Documents.DoneWorkReport",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id }
				}
			};
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Акт выполненных работ");

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		int copiesToPrint = 2;
		public override int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}
	}
}

