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
			Map(x => x.CallTime).Column("call_time");
			Map(x => x.Phone).Column("phone");
			Map(x => x.Status).Column("status").CustomType<RoboatsCallStatusStringType>();
			Map(x => x.FailType).Column("fail_type").CustomType<RoboatsCallFailTypeStringType>();
			Map(x => x.Operation).Column("operation").CustomType<RoboatsCallOperationStringType>();
			Map(x => x.Result).Column("result").CustomType<RoboatsCallResultStringType>();
			Map(x => x.Description).Column("description");
		}
	}
}
