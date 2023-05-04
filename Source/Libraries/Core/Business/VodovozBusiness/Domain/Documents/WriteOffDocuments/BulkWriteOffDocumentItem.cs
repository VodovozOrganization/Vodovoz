using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Documents.IncomingInvoices;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания (объемный учет)",
		Nominative = "строка списания (объемный учет)")]
	[HistoryTrace]
	public abstract class BulkWriteOffDocumentItem : WriteOffDocumentItem
	{
		public override AccountingType AccountingType => AccountingType.Bulk;
	}
}

