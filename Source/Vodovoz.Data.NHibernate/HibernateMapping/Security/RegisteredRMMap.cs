using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Security
{
	public class RegisteredRMMap : ClassMap<RegisteredRM>
	{
		public RegisteredRMMap()
		{
			Table("registered_rms");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Username).Column("rm_name");
			Map(x => x.Domain).Column("domain_name");
			Map(x => x.SID).Column("user_sid");
			Map(x => x.IsActive).Column("is_active");

			HasManyToMany(x => x.Users)
								.Table("user_rm_restrictions")
								.ParentKeyColumn("registered_rm_id")
								.ChildKeyColumn("user_id")
								.LazyLoad();
		}
	}
}
