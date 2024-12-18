using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Permissions
{
	public class PresetPermissionBaseMap : ClassMap<HierarchicalPresetPermissionBase>
	{
		public PresetPermissionBaseMap()
		{
			Table("permission_preset");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("permission_type");
			Map(x => x.PresetPermissionType).Column("permission_type").Update().Not.Insert();

			Map(x => x.PermissionName).Column("permission_name");
			Map(x => x.Value).Column("value");
		}
	}
}
