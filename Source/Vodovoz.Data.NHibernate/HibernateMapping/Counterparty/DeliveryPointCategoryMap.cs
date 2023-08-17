using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class DeliveryPointCategoryMap : ClassMap<DeliveryPointCategory>
	{
		public DeliveryPointCategoryMap()
		{
			Table("delivery_point_categories");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}