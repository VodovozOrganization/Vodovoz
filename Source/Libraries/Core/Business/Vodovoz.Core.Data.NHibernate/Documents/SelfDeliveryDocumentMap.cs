using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class SelfDeliveryDocumentMap : ClassMap<SelfDeliveryDocumentEntity>
	{
		public SelfDeliveryDocumentMap()
		{
			Table("store_self_delivery_document");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Order).Column("order_id");
		}
	}
}
