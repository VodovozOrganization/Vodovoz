using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class ServiceDistrictsSetMap : ClassMap<ServiceDistrictsSet>
	{
		public ServiceDistrictsSetMap()
		{
			Table("service_districts_sets");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.DateCreated).Column("date_created");
			Map(x => x.DateActivated).Column("date_activated");
			Map(x => x.DateClosed).Column("date_closed");
			Map(x => x.Comment).Column("comment");
			Map(x => x.Status).Column("status");

			References(x => x.Author).Column("author_id");

			HasMany(x => x.ServiceDistricts).Cascade.AllDeleteOrphan().Inverse().KeyColumn("service_districts_set_id");
		}
	}
}
