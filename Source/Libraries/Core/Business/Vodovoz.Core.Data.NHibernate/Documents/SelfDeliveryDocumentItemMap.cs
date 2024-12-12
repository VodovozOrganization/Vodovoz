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
		}
	}
}
