using System;
using System.Collections.Generic;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class UPDDocument : PrintableOrderDocument, IPrintableRDLDocument, ISignableDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.UPD;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = String.Format("УПД {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.UPD",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "special", false },
					{ "hide_signature", HideSignature}
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("УПД №{0}", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;

		public virtual bool HideSignature { get; set; } = true;

		private int copiesToPrint = -1;
		public override int CopiesToPrint
		{
			get {
				if (copiesToPrint < 0)
				{
					if (Order.PaymentType == PaymentType.BeveragesWorld && Order.Client.UPDCount.HasValue)
						return Order.Client.UPDCount.Value;
					
					return Order.DocumentType.HasValue && Order.DocumentType.Value == DefaultDocumentType.torg12 ? 1 : 2;
				}

				return copiesToPrint;
			}
			set => copiesToPrint = value;
		}
	}
}

