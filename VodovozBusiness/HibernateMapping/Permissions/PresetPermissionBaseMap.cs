using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.HibernateMapping.Permissions
{
	public class PresetPermissionBaseMap : ClassMap<PresetPermissionBase>
	{
		public PresetPermissionBaseMap()
		{
			Table("permission_preset_user");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("permission_type");
			Map(x => x.PresetPermissionType).CustomType<PresetPermissionTypeCustomType>().Update().Not.Insert(); ;

			Map(x => x.PermissionName).Column("permission_name");
			Map(x => x.Value).Column("value");
		}
	}
}
