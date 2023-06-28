using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class UndeliveryObjectMap : ClassMap<UndeliveryObject>
	{
		public UndeliveryObjectMap()
		{
			Table("undelivery_objects");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
