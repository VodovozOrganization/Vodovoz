using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class ShetFacturaDocumentEntity : PrintableOrderDocumentEntity
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.ShetFactura;
		#endregion

		public override string Name => $"Счет-Фактура №{Order.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;
	}
}
