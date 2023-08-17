using FluentNHibernate.Mapping;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.HibernateMapping.Roboats
{
	public class RoboatsCallDetailMap : ClassMap<RoboatsCallDetail>
	{
		public RoboatsCallDetailMap()
		{
			Table("roboats_call_details");

			Id(x => x.Id).GeneratedBy.Native();
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.FailType).Column("fail_type").CustomType<RoboatsCallFailTypeStringType>();
			Map(x => x.Operation).Column("operation").CustomType<RoboatsCallOperationStringType>();
			Map(x => x.Description).Column("description");
			
			References(x => x.Call).Column("call_id")
				.Fetch.Join()
				.Not.LazyLoad();
		}
	}
}
