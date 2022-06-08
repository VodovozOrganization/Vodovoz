using FluentNHibernate.Mapping;
using QS.HistoryLog.Domain;
using Vodovoz.Domain.HistoryChanges;

namespace Vodovoz.HibernateMapping.HistoryChanges
{
	public class OldChangedEntityMap : ClassMap<OldChangedEntity>
	{
		public OldChangedEntityMap()
		{
			Schema("Vodovoz_old_monitoring");
			Table("history_changed_entities");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ChangeTime).Column("datetime");
			Map(x => x.Operation).Column("operation").CustomType<EntityChangeOperationStringType>();
			Map(x => x.EntityClassName).Column("entity_class");
			Map(x => x.EntityId).Column("entity_id");
			Map(x => x.EntityTitle).Column("entity_title");

			References(x => x.ChangeSet).Column("changeset_id").Not.LazyLoad();

			HasMany(x => x.Changes).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("changed_entity_id");
		}
	}
}
