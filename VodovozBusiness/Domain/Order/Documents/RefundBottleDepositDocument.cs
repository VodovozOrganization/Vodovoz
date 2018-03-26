using System;
using System.Collections.Generic;
using QSReport;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Domain.Orders.Documents
{
	public class RefundBottleDepositDocument : OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override QSReport.ReportInfo GetReportInfo()
		{
			return new QSReport.ReportInfo {
				Title = String.Format("Акт возврата залогов за бутыли"),
				Identifier = "Documents.RefundBottleDeposit",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id }
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.RefundBottleDeposit;
			}
		}

		#endregion

		public override string Name { get { return String.Format("Акт возврата залогов за бутыли"); } }

		public override DateTime? DocumentDate {
			get { return Order?.DeliveryDate; }
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}

		public override DocumentOrientation Orientation {
			get {
				return DocumentOrientation.Portrait;
			}
		}
	}
}
