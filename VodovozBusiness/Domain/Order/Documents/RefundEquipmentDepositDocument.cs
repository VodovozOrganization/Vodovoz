using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Domain.Orders.Documents
{
	public class RefundEquipmentDepositDocument : OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.RefundEquipmentDeposit;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = String.Format("Акт возврата залогов за оборудование"),
				Identifier = "Documents.RefundEquipmentDeposit",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id }
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
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
