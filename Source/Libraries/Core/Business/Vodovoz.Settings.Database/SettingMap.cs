using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace Vodovoz.Settings.Database
{
	public class SettingMap : ClassMap<Setting>
	{
		public SettingMap()
		{
			Table("base_parameters");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			Map(x => x.StrValue).Column("str_value");
			Map(x => x.CacheTimeout).Column("cache_timeout").CustomType<TimeAsTimeSpanType>();
		}
	}
}
