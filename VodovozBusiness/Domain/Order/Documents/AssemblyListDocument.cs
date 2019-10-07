using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class AssemblyListDocument: OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.AssemblyList;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = String.Format($"Лист сборки от {Order.DeliveryDate:d}"),
				Identifier = (Order.OrderItems?.Count ?? 0) <= 4 ? "Documents.AssemblyList" : "Documents.SeparateAssemblyList",
				Parameters = new Dictionary<string, object>
				{
					{ "order_id",  Order.Id}
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format($"Лист сборки от от {Order.DeliveryDate:d}");

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint => 1;
	}
}
