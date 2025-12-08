using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class InvoiceBarterDocumentEntity : PrintableOrderDocumentEntity
	{
		private int _copiesToPrint = 1;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.InvoiceBarter;
		#endregion

		public override string Name => $"Накладная №{Order.Id} (бартер)";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}
	}
}
