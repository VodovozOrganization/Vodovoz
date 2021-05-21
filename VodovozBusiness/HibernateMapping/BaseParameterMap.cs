using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping
{
	public class BaseParameterMap : ClassMap<BaseParameter>
	{
		public BaseParameterMap()
		{
			Table("base_parameters");

			Id(x => x.Name).Column("name").GeneratedBy.Assigned();

			Map(x => x.StrValue).Column("str_value");
			Map(x => x.CacheTimeout).Column("cache_timeout").CustomType<TimeAsTimeSpanType>();
		}
	}
}
