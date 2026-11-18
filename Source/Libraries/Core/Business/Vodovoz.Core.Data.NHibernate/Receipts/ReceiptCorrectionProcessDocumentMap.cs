using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Data.NHibernate.Receipts
{
	public class ReceiptCorrectionProcessDocumentMap : ClassMap<ReceiptCorrectionProcessDocument>
	{
		public ReceiptCorrectionProcessDocumentMap()
		{
			Table("receipt_correction_process_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Process).Column("receipt_correction_process_id");
			Map(x => x.PlannedDocumentType).Column("planned_document_type").CustomType<EnumType<FiscalDocumentType>>();
			Map(x => x.DocumentGuid).Column("document_guid");
			Map(x => x.EdoFiscalDocumentId).Column("edo_fiscal_document_id");
			Map(x => x.Status).Column("status").CustomType<EnumType<ReceiptCorrectionProcessStatus>>();
			Map(x => x.ErrorDescription).Column("error_description");
		}
	}
}
