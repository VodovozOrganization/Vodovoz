using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class EquipmentReturnDocument:OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.EquipmentReturn;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = Name,
				Identifier = "Documents.EquipmentReturn",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id }
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Акт закрытия аренды");

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

