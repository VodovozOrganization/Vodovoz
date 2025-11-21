using QS.Print;
using System;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	/// <summary>
	/// Документ - Лист сборки
	/// </summary>
	public class AssemblyListDocumentEntity : PrintableOrderDocumentEntity
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.AssemblyList;
		#endregion

		public override string Name => $"Лист сборки от от {Order.DeliveryDate:d}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint => 1;
	}
}
