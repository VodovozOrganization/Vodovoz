using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения(объемный учет)",
		Nominative = "строка перемещения(объемный учет)")]
	public abstract class BulkMovementDocumentItem : MovementDocumentItem
	{
		public override AccountingType AccountingType => AccountingType.Bulk;
	}
}
