using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class TransportInvoiceDocumentEntity : PrintableOrderDocumentEntity
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.TransportInvoice;
		#endregion

		public override string Name => $"Товарно-транспортная накладная от {Order.DeliveryDate:d}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint => Order.Client.TTNCount ?? 1;

		public TransportInvoiceDocumentEntity()
		{
		}
	}
}
