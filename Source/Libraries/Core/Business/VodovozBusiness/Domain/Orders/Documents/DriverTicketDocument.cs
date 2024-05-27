﻿using System;
using System.Collections.Generic;
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
			return new ReportInfo {
				Title = String.Format("Талон водителю {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.DriverTicket",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "contactless_delivery", Order.ContactlessDelivery}
				}
			};
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Талон водителю №{0}", Order.Id);

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

