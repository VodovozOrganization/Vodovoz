using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class SettingsMap : ClassMap<PacsDomainSettings>
	{
		public SettingsMap()
		{
			Table("pacs_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Timestamp).Column("timestamp").ReadOnly();
			Map(x => x.AdministratorId).Column("administrator_id");
			Map(x => x.MaxBreakTime).Column("max_operator_break_time");
			Map(x => x.MaxOperatorsOnBreak).Column("max_operators_on_break");
		}
	}
}
