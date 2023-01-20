using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания (объемный учет)",
		Nominative = "строка списания (объемный учет)")]
	[HistoryTrace]
	public class BulkWriteOffDocumentItem : WriteOffDocumentItem
	{
		public override AccountingType AccountingType => AccountingType.Bulk;
	}
}

