using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Users
{
	public class AvailableDatabaseMap : ClassMap<AvailableDatabase>
	{
		public AvailableDatabaseMap()
		{
			Schema("Vodovoz_admin_parameters");
			Table("available_databases");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");

			HasManyToMany(x => x.UserRoles)
				.Schema("Vodovoz_admin_parameters")
				.Table("user_roles_available_databases")
				.ParentKeyColumn("available_database_id")
				.ChildKeyColumn("user_role_id")
				.LazyLoad();

			HasManyToMany(x => x.UnavailableForPrivilegeNames)
				.Schema("Vodovoz_admin_parameters")
				.Table("unavailable_databases_for_privilege_names")
				.ParentKeyColumn("available_database_id")
				.ChildKeyColumn("privilege_name_id")
				.LazyLoad();
		}
	}
}
