using System;
using System.Collections.Generic;
using Autofac;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Domain.Orders.Documents
{
	public class DoneWorkDocument : PrintableOrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.DoneWorkReport;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = Name;
			reportInfo.Identifier = "Documents.DoneWorkReport";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id",  Order.Id }
			};
			return reportInfo;
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"Акт выполненных работ №{DocumentOrganizationCounter.DocumentNumber}"
			:  $"Акт выполненных работ";

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

