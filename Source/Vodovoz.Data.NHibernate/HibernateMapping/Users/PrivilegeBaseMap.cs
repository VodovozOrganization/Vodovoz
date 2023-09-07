using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Users
{
	public class PrivilegeBaseMap : ClassMap<PrivilegeBase>
	{
		public PrivilegeBaseMap()
		{
			Schema("Vodovoz_admin_parameters");
			Table("privileges");
			DiscriminateSubClassesOnColumn("privilege_type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DatabaseName).Column("database_name");
			Map(x => x.TableName).Column("table_name");

			References(x => x.UserRole).Column("user_role_id");
			References(x => x.PrivilegeName).Column("privilege_name_id");
		}
	}
}
