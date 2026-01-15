using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class SpecialUPDDocumentEntity : PrintableOrderDocumentEntity, ISignableDocument
	{
		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"Специальный УПД №{DocumentOrganizationCounter?.DocumentNumber ?? "-"}"
			:  $"Специальный УПД №{Order?.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;
		
		public override OrderDocumentType Type => OrderDocumentType.SpecialUPD;

		/// <summary>
		/// Без подписей и печати
		/// </summary>
		public virtual bool HideSignature { get; set; } = true;
	}
}
