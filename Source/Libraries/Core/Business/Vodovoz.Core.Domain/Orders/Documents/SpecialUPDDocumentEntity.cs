using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class SpecialUPDDocumentEntity : PrintableOrderDocumentEntity, ISignableDocument
	{
		public override string Name => $"Особый УПД №{Order.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;

		/// <summary>
		/// Без подписей и печати
		/// </summary>
		public virtual bool HideSignature { get; set; } = true;
	}
}
