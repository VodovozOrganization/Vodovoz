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
			Map(x => x.MaxBreakTime).Column("max_operator_break_time").CustomType<TimeAsTimeSpanType>();
			Map(x => x.MaxOperatorsOnBreak).Column("max_operators_on_break");
		}
	}
}
