using System;
using System.Collections.Generic;
using Autofac;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Domain.Orders.Documents
{
	public class DriverTicketDocument : PrintableOrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.DriverTicket;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = $"{Name} от {Order.DeliveryDate:d}";
			reportInfo.Identifier = "Documents.DriverTicket";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id",  Order.Id },
				{ "contactless_delivery", Order.ContactlessDelivery}
			};
			return reportInfo;
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"Талон водителю №{DocumentOrganizationCounter?.DocumentNumber ?? Order?.Id.ToString()}"
			:  $"Талон водителю №{Order?.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		int copiesToPrint = 1;
		public override int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}
	}
}

