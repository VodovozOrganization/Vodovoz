using FluentNHibernate.Mapping;
using Vodovoz.Domain.HistoryChanges;

namespace Vodovoz.HibernateMapping.HistoryChanges
{
	public class UserForOldMonitoringMap : ClassMap<UserForOldMonitoring>
	{
		public UserForOldMonitoringMap()
		{
			Schema("Vodovoz_honeybee");
			Table("users");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Login).Column("login");
			Map(x => x.Deactivated).Column("deactivated");
			Map(x => x.IsAdmin).Column("admin");
			Map(x => x.Email).Column("email");
		}
	}
}
