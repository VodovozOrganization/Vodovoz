using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class EquipmentTransferDocument:OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.EquipmentTransfer;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = Name,
				//Identifier = "Documents.EquipmentMovement",
				//FIXME Когда доделаем навороченый акт I-1200 вернуть
				Identifier = "Documents.EquipmentTransfer",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id }
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name {
			get { return String.Format("Акт приемо-передачи оборудования"); }
			//FIXME Когда доделаем навороченый акт I-1200 вернуть
			//get { return String.Format ("Акт движения оборудования"); }
		}

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

