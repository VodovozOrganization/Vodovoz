using QS.DomainModel.Entity;
using Vodovoz.Domain.Documents.IncomingInvoices;

namespace Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения(объемный учет)",
		Nominative = "строка перемещения(объемный учет)")]
	public abstract class BulkMovementDocumentItem : MovementDocumentItem
	{
		public override AccountingType AccountingType => AccountingType.Bulk;
	}
}
