using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class SelfDeliveryDocumentMap : ClassMap<SelfDeliveryDocumentEntity>
	{
		public SelfDeliveryDocumentMap()
		{
			Table("store_self_delivery_document");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.TimeStamp)
				.Column("time_stamp");
			Map(x => x.AuthorId)
				.Column("author_id");
			Map(x => x.LastEditorId)
				.Column("last_editor_id");
			Map(x => x.LastEditedTime)
				.Column("last_edit_time");

			References(x => x.Order)
				.Column("order_id");

			References(x => x.Warehouse)
				.Column("warehouse_id");

			HasMany(x => x.Items)
				.Cascade
				.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("store_self_delivery_document_id");
		}
	}
}
