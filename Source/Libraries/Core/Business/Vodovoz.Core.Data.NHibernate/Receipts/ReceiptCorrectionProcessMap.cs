using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Data.NHibernate.Receipts
{
	public class ReceiptCorrectionProcessMap : ClassMap<ReceiptCorrectionProcess>
	{
		public ReceiptCorrectionProcessMap()
		{
			Table("receipt_correction_processes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.OrderId).Column("order_id");
			Map(x => x.SourceEdoFiscalDocumentId).Column("source_edo_fiscal_document_id");
			Map(x => x.ReceiptEdoTaskId).Column("receipt_edo_task_id");
			Map(x => x.BaselineFiscalDocumentNumber).Column("baseline_fiscal_document_number");
			Map(x => x.BaselineFiscalDocumentDate).Column("baseline_fiscal_document_date");
			Map(x => x.BaselineSum).Column("baseline_sum");
			Map(x => x.ScenarioType).Column("scenario_type").CustomType<EnumType<ReceiptCorrectionScenarioType>>();
			Map(x => x.Status).Column("status").CustomType<EnumType<ReceiptCorrectionProcessStatus>>();
			Map(x => x.ChangeFingerprint).Column("change_fingerprint");
			Map(x => x.ErrorDescription).Column("error_description");
			Map(x => x.CreatedDate).Column("created_date");
			Map(x => x.CompletedDate).Column("completed_date");
			Map(x => x.OrganizationId).Column("organization_id");

			HasMany(x => x.Documents)
				.KeyColumn("receipt_correction_process_id")
				.Cascade.AllDeleteOrphan()
				.Inverse();

			HasMany(x => x.ExplanatoryNotes)
				.KeyColumn("receipt_correction_process_id")
				.Cascade.AllDeleteOrphan()
				.Inverse();
		}
	}
}
