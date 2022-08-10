using FluentNHibernate.Mapping;
using Vodovoz.Domain.HistoryChanges;

namespace Vodovoz.HibernateMapping.HistoryChanges
{
	public class ArchiveChangeSetMap : ClassMap<ArchiveChangeSet>
	{
		public ArchiveChangeSetMap()
		{
			Schema("Vodovoz_old_monitoring");
			Table("history_changeset");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.UserLogin).Column("user_login");
			Map(x => x.ActionName).Column("action_name");

			References(x => x.User).Column("user_id").Not.LazyLoad();

			HasMany(x => x.Entities).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("changeset_id");
		}
	}
}
