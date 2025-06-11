using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Users
{
	public class UserRoleMap : ClassMap<UserRole>
	{
		public UserRoleMap()
		{
			Schema("Vodovoz_admin_parameters");
			Table("user_roles");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Description).Column("description");

			HasMany(x => x.Privileges).Inverse().Cascade.AllDeleteOrphan().KeyColumn("user_role_id");

			HasManyToMany(x => x.AvailableDatabases)
				.Schema("Vodovoz_admin_parameters")
				.Table("user_roles_available_databases")
				.ParentKeyColumn("user_role_id")
				.ChildKeyColumn("available_database_id")
				.LazyLoad();
		}
	}
}
