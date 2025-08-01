using System.Collections.Generic;
using System.Linq;
using Autofac;
using MoreLinq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	public class CurrentWarehousePermissions
	{
		private IList<WarehousePermissionBase> permissions;

		public IList<WarehousePermissionBase> WarehousePermissions
		{
			get
			{
				if(permissions == null)
				{
					Load();
				}

				return permissions;
			}
		}

		private void Load()
		{
			var userId = ServicesConfig.UserService.CurrentUserId;
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateForRoot<User>(userId))
			{
				var employee = ScopeProvider.Scope.Resolve<IEmployeeRepository>().GetEmployeeForCurrentUser(uow);
				var subdivision = employee.Subdivision;
				permissions = new List<WarehousePermissionBase>();
				var userWarehousePermissionsQuery = uow.Session.QueryOver<UserWarehousePermission>()
					.Where(x => x.User.Id == userId).List();
				userWarehousePermissionsQuery.ForEach(x => permissions.Add(x));
				while(subdivision != null)
				{
					var subdivisionWarehousePermissionQuery = uow.Session.QueryOver<SubdivisionWarehousePermission>()
						.Where(x => x.Subdivision.Id == subdivision.Id
							&& !x.Warehouse.Id.IsIn(permissions.Select(y => y.Warehouse.Id).Distinct().ToArray())).List();
					subdivisionWarehousePermissionQuery.ForEach(x => permissions.Add(x));
					subdivision = subdivision.ParentSubdivision;
				}
				// permissions.Distinct()
			}
		}
	}
}
