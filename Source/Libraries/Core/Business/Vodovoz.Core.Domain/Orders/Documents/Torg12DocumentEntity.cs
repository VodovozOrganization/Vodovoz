using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class Torg12DocumentEntity : PrintableOrderDocumentEntity
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Torg12;
		#endregion

		public override string Name => $"ТОРГ-12 №{Order.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;
	}
}
