using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class UserMap : ClassMap<User>
	{
		public UserMap()
		{
			Table("users");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Login).Column("login");
			Map(x => x.Deactivated).Column("deactivated");
			Map(x => x.IsAdmin).Column("admin");
			Map(x => x.Email).Column("email");
			Map(x => x.NeedPasswordChange).Column("need_password_change");
			Map(x => x.Description).Column("description");

			Map(x => x.WarehouseAccess).Column("warehouse_access").LazyLoad();

			HasManyToMany(x => x.RegisteredRMs)
					.Table("user_rm_restrictions")
					.ParentKeyColumn("user_id")
					.ChildKeyColumn("registered_rm_id")
					.LazyLoad();
		}
	}
}
