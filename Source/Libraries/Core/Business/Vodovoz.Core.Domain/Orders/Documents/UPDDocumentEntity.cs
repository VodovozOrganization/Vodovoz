using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class UPDDocumentEntity : PrintableOrderDocumentEntity, ISignableDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.UPD;
		#endregion

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"УПД №{DocumentOrganizationCounter.DocumentNumber}"
			:  $"УПД №{Order?.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;

		public virtual bool HideSignature { get; set; } = true;
	}
}
