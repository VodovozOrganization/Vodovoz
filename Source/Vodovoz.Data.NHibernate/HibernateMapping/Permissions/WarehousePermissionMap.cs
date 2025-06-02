using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Permissions
{
	public class WarehousePermissionMap : ClassMap<WarehousePermissionBase>
	{
		public WarehousePermissionMap()
		{
			Table("warehouse_permissions");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("type_permission");

			Map(x => x.PermissionValue).Column("value");
			Map(x => x.WarehousePermissionType).Column("warehouse_permission_type");
			References(x => x.Warehouse).Column("warehouse_id");
		}
	}

	public class UserWarehousePermissionMap : SubclassMap<UserWarehousePermission>
	{
		public UserWarehousePermissionMap()
		{
			DiscriminatorValue("User");

			References(x => x.User).Column("user_id");
		}
	}

	public class SubdivisionWarehousePermissionMap : SubclassMap<SubdivisionWarehousePermission>
	{
		public SubdivisionWarehousePermissionMap()
		{
			DiscriminatorValue("Subdivision");

			References(x => x.Subdivision).Column("subdivision_id");
		}
	}
}
