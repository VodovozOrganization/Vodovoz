using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class SelfDeliveryDocumentItemMap : ClassMap<SelfDeliveryDocumentItemEntity>
	{
		public SelfDeliveryDocumentItemMap()
		{
			Table("store_self_delivery_document_item");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.SelfDeliveryDocument).Column("store_self_delivery_document_id");

			HasMany(x => x.TrueMarkProductCodes).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("self_delivery_document_item_id");
		}
	}
}
