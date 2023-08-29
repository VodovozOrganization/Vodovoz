using FluentNHibernate.Mapping;
using Vodovoz.Domain.HistoryChanges;

namespace Vodovoz.Data.NHibernate.HibernateMapping.HistoryChanges
{
	public class ArchivedChangedEntityMap : ClassMap<ArchivedChangedEntity>
	{
		public ArchivedChangedEntityMap()
		{
			Schema("Vodovoz_old_monitoring");
			Table("history_changed_entities");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ChangeTime).Column("datetime");
			Map(x => x.Operation).Column("operation");
			Map(x => x.EntityClassName).Column("entity_class");
			Map(x => x.EntityId).Column("entity_id");
			Map(x => x.EntityTitle).Column("entity_title");

			References(x => x.ChangeSet).Column("changeset_id").Not.LazyLoad();

			HasMany(x => x.Changes).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("changed_entity_id");
		}
	}
}
