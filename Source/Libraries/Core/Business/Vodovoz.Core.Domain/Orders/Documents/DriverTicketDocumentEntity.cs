using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class DriverTicketDocumentEntity : PrintableOrderDocumentEntity
	{
		private int _copiesToPrint = 1;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.DriverTicket;
		#endregion
		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"Талон водителю №{DocumentOrganizationCounter?.DocumentNumber ?? "-"}"
			:  $"Талон водителю №{Order?.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}
	}
}
