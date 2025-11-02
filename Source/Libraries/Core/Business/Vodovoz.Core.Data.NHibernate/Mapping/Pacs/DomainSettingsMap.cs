using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class DomainSettingsMap : ClassMap<DomainSettings>
	{
		public DomainSettingsMap()
		{
			Table("pacs_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Timestamp).Column("timestamp").ReadOnly();
			Map(x => x.AdministratorId).Column("administrator_id");
			Map(x => x.OperatorsOnLongBreak).Column("long_break_operators");
			Map(x => x.LongBreakDuration).Column("long_break_duration").CustomType<TimeAsTimeSpanType>();
			Map(x => x.LongBreakCountPerDay).Column("long_break_count");
			Map(x => x.OperatorsOnShortBreak).Column("short_break_operators");
			Map(x => x.ShortBreakDuration).Column("short_break_duration").CustomType<TimeAsTimeSpanType>();
			Map(x => x.ShortBreakInterval).Column("short_break_interval").CustomType<TimeAsTimeSpanType>();
		}
	}
}
