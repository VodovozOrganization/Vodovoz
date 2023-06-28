using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class CamplaintDetalizationMap : ClassMap<UndeliveryDetalization>
	{
		public CamplaintDetalizationMap()
		{
			Table("undelivery_detalizations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");

			References(x => x.UndeliveryKind).Column("undelivery_kind_id");
		}
	}
}
