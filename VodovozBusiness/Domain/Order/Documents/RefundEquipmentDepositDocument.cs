using System;
using System.Collections.Generic;
using QSReport;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Domain.Orders.Documents
{
	public class RefundEquipmentDepositDocument : OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override QSReport.ReportInfo GetReportInfo()
		{
			return new QSReport.ReportInfo {
				Title = String.Format("Акт возврата залогов за оборудование"),
				Identifier = "Documents.RefundEquipmentDeposit",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id }
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.RefundEquipmentDeposit;
			}
		}

		#endregion

		public override string Name { get { return String.Format("Акт возврата залогов за оборудование"); } }

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
