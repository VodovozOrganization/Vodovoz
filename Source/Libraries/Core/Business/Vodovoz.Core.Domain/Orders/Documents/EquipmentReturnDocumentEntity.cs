using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class EquipmentReturnDocumentEntity : PrintableOrderDocumentEntity
	{
		private int _copiesToPrint = 2;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.EquipmentReturn;
		#endregion

		public override string Name => "Акт закрытия аренды";

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
