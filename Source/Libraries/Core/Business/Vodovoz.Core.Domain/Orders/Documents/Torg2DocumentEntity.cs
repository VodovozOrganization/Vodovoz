using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class Torg2DocumentEntity : PrintableOrderDocumentEntity
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Torg2;
		#endregion
		public override string Name => $"Торг-2 от {Order.DeliveryDate:d}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint => Order.Client.Torg2Count ?? 1;
	}
}
