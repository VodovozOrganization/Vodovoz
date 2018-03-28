using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSReport;
using Vodovoz.Domain.Service;

namespace Vodovoz.Domain.Orders.Documents
{
	public class EquipmentTransferDocument:OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = Name,
				Identifier = "Documents.EquipmentTransfer",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id }
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.EquipmentTransfer;
			}
		}

		#endregion

		public override string Name {
			get { return String.Format ("Акт приема-передачи оборудования"); }
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

