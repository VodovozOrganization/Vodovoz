using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.HibernateMapping.Documents.WriteOffDocuments
{
	public class WriteOffDocumentItemMap : ClassMap<WriteOffDocumentItem>
	{
		public WriteOffDocumentItemMap()
		{
			Table("store_writeoff_document_items");
			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");
			Map(x => x.Comment).Column("comment");

			References(x => x.Fine).Column("fine_id");
			References(x => x.Document).Column("write_off_document_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.CullingCategory).Column("culling_category_id");
			References(x => x.GoodsAccountingOperation)
				.Column("write_off_goods_accounting_operation_id")
				.Cascade.All();
		}
	}
}
