using FluentNHibernate.Mapping;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.HibernateMapping.Roboats
{
	public class RoboatsCallMap : ClassMap<RoboatsCall>
	{
		public RoboatsCallMap()
		{
			Table("roboats_call_registry");

			Id(x => x.Id).GeneratedBy.Native();
			Map(x => x.CallGuid).Column("call_guid");
			Map(x => x.CallTime).Column("call_time");
			Map(x => x.Phone).Column("phone");
			Map(x => x.Status).Column("status").CustomType<RoboatsCallStatusStringType>();
			Map(x => x.Result).Column("result").CustomType<RoboatsCallResultStringType>();
			
			HasMany(x => x.CallDetails).KeyColumn("call_id")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.Not.LazyLoad();
		}
	}
}
