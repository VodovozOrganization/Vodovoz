using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark
{
	public class StagingTrueMarkCodeMap : ClassMap<StagingTrueMarkCode>
	{
		public StagingTrueMarkCodeMap()
		{
			Table("true_mark_code_staging");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ParentCodeId).Column("parent_code_id");
			Map(x => x.RawCode).Column("raw_code");
			Map(x => x.Gtin).Column("gtin");
			Map(x => x.SerialNumber).Column("serial_number");
			Map(x => x.CheckCode).Column("check_code");
			Map(x => x.CodeType).Column("code_type");
			Map(x => x.RelatedDocumentType).Column("related_document_type");
			Map(x => x.RelatedDocumentId).Column("related_document_id");
			Map(x => x.OrderItemId).Column("order_item_id");

			HasMany(x => x.InnerCodes)
				.KeyColumn("parent_code_id")
				.Not.LazyLoad()
				.Fetch.Subselect()
				.Inverse()
				.Cascade.All();
		}
	}
}
