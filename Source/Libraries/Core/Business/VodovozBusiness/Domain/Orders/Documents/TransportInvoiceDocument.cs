using System;
using System.Collections.Generic;
using Autofac;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Domain.Orders.Documents
{
	public class TransportInvoiceDocument : PrintableOrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.TransportInvoice;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Identifier = "Documents.TransportInvoice";
			reportInfo.Title = String.Format($"Товарно-транспортная накладная от {Order.DeliveryDate:d}");
			reportInfo.Parameters = new Dictionary<string, object> {
				{ "order_id",  Order.Id }
			};
			return reportInfo;
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format($"Товарно-транспортная накладная от {Order.DeliveryDate:d}");

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint => Order.Client.TTNCount ?? 1;

		public TransportInvoiceDocument()
		{
		}
	}
}
