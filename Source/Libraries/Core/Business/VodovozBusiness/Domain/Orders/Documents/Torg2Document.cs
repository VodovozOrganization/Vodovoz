using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class Torg2Document : PrintableOrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Torg2;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			return new ReportInfo {
				Title = String.Format($"Торг-2 от {Order.DeliveryDate:d}"),
				Identifier = "Documents.Torg2",
				Parameters = new Dictionary<string, object> 
				{
					{ "document_id",  Id}
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format($"Торг-2 от {Order.DeliveryDate:d}");

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint => Order.Client.Torg2Count ?? 1;

		Dictionary<object, object> IPrintableRDLDocument.Parameters { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}
}
