﻿using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Domain.Orders.Documents
{
	public class Torg12Document : PrintableOrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Torg12;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			return new ReportInfo {
				Title = String.Format("ТОРГ-12 {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.Torg12",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id.ToString() },
					{ "without_vat", Order.IsCashlessPaymentTypeAndOrganizationWithoutVAT }
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("ТОРГ-12 №{0}", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;

	}
}

