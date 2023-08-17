using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class UndeliveryKindMap : ClassMap<UndeliveryKind>
	{
		public UndeliveryKindMap()
		{
			Table("undelivery_kinds");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");

			References(x => x.UndeliveryObject).Column("undelivery_object_id");
		}
	}
}
