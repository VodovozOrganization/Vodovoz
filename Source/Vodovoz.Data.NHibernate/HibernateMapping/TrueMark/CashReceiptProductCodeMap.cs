using FluentNHibernate.Mapping;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Data.NHibernate.HibernateMapping.TrueMark
{
	public class CashReceiptProductCodeMap : ClassMap<CashReceiptProductCode>
	{
		public CashReceiptProductCodeMap()
		{
			Table("cash_receipt_product_codes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.CashReceipt).Column("cash_receipt_id");
			References(x => x.OrderItem).Column("order_item_id");
			Map(x => x.IsUnscannedSourceCode).Column("is_unscanned_source_code");
			Map(x => x.IsDefectiveSourceCode).Column("is_defective_source_code");
			Map(x => x.IsDuplicateSourceCode).Column("is_duplicate_source_code");
			Map(x => x.DuplicatedIdentificationCodeId).Column("duplicated_identification_code_id");
			Map(x => x.DuplicatsCount).Column("duplicates_count");
			References(x => x.SourceCode).Column("source_identification_code_id");
			References(x => x.ResultCode).Column("result_identification_code_id");
		}
	}
}
