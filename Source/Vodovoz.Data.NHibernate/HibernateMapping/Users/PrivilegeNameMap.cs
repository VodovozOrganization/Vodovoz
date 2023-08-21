﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.HibernateMapping.Users
{
	public class PrivilegeNameMap : ClassMap<PrivilegeName>
	{
		public PrivilegeNameMap()
		{
			Schema("Vodovoz_admin_parameters");
			Table("privilege_names");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.PrivilegeType).Column("privilege_type");

			HasManyToMany(x => x.UnavailableDatabases)
				.Table("unavailable_databases_for_privilege_names")
				.ParentKeyColumn("privilege_name_id")
				.ChildKeyColumn("available_database_id")
				.LazyLoad();
		}
	}
}
