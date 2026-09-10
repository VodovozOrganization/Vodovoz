using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Data.NHibernate.Receipts
{
	public class ReceiptCorrectionExplanatoryNoteMap : ClassMap<ReceiptCorrectionExplanatoryNote>
	{
		public ReceiptCorrectionExplanatoryNoteMap()
		{
			Table("receipt_correction_explanatory_notes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Process).Column("receipt_correction_process_id");
			Map(x => x.TemplateType).Column("template_type").CustomType<EnumType<ReceiptCorrectionExplanatoryNoteTemplateType>>();
			Map(x => x.OrganizationId).Column("organization_id");
			Map(x => x.SignerTitle).Column("signer_title");
			Map(x => x.SignerName).Column("signer_name");
			Map(x => x.SignerSignatureId).Column("signer_signature_id");
			Map(x => x.Content).Column("content").CustomSqlType("TEXT");
			Map(x => x.CreatedDate).Column("created_date");
		}
	}
}
