using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class DoneWorkDocumentEntity : PrintableOrderDocumentEntity
	{
		private int _copiesToPrint = 2;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.DoneWorkReport;
		#endregion

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"Акт выполненных работ №{DocumentOrganizationCounter.DocumentNumber}"
			:  $"Акт выполненных работ";

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
