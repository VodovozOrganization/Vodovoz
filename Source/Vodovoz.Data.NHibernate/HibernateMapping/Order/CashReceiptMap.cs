using FluentNHibernate.Mapping;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class CashReceiptMap : ClassMap<CashReceipt>
	{
		public CashReceiptMap()
		{
			Table("cash_receipts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Order).Not.LazyLoad().Column("order_id");

			Map(x => x.CreateDate).Column("start_date");
			Map(x => x.UpdateDate).Column("update_date").ReadOnly();
			Map(x => x.Status).Column("status");
			Map(x => x.UnscannedCodesReason).Column("unscanned_codes_reason");
			Map(x => x.ErrorDescription).Column("error_description");
			Map(x => x.FiscalDocumentStatus).Column("fiscal_document_status");
			Map(x => x.FiscalDocumentNumber).Column("fiscal_document_number");
			Map(x => x.FiscalDocumentDate).Column("fiscal_document_date");
			Map(x => x.FiscalDocumentStatusChangeTime).Column("fiscal_document_status_change_time");
			Map(x => x.Sum).Column("sum");
			Map(x => x.ManualSent).Column("manual_sent");
			Map(x => x.Contact).Column("contact");
			Map(x => x.WithoutMarks).Column("without_marks");
			Map(x => x.InnerNumber).Column("inner_number");
			Map(x => x.CashboxId).Column("cashbox_id");
			Map(x => x.EdoTaskId).Column("edo_task_id");

			HasMany(x => x.ScannedCodes).Cascade.AllDeleteOrphan().Not.LazyLoad().Inverse()
				.KeyColumn("cash_receipt_id");
		}
	}
}
