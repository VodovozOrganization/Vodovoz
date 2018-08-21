using System;
using System.Collections.Generic;
using QS.Print;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	public class EquipmentReturnDocument:OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument

		public virtual QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = Name,
				Identifier = "Documents.EquipmentReturn",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id }
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.EquipmentReturn;
			}
		}

		#endregion

		public override string Name {
			get { return String.Format("Акт закрытия аренды"); }
		}

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

