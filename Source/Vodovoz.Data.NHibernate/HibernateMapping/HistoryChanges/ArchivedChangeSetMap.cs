using FluentNHibernate.Mapping;
using Vodovoz.Domain.HistoryChanges;

namespace Vodovoz.Data.NHibernate.HibernateMapping.HistoryChanges
{
	public class ArchivedChangeSetMap : ClassMap<ArchivedChangeSet>
	{
		public ArchivedChangeSetMap()
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
