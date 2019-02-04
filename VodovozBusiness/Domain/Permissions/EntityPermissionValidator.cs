using System;
using System.Reflection;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Domain.Employees;
using System.Linq;
using Vodovoz.Repositories.Permissions;

namespace Vodovoz.Domain.Permissions
{
	public class EntityPermissionValidator : QS.DomainModel.Entity.EntityPermissions.EntityPermissionValidator
	{
		public override EntityPermission Validate<TEntityType>(int userId)
		{
			return Validate(typeof(TEntityType), userId);
		}

		public override EntityPermission Validate(Type entityType, int userId)
		{
			var permission = base.Validate(entityType, userId);
			if(!permission.IsEmpty) {
				return permission;
			}

			var deniedPermission = new EntityPermission(false, false, false, false);
			var permissionAttr = entityType.GetCustomAttribute<EntityPermissionAttribute>();
			if(permissionAttr == null) {
				return deniedPermission;
			}

			Employee employee;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				employee = EmployeeRepository.GetEmployeesForUser(uow, userId).FirstOrDefault();

				if(employee == null || employee.Subdivision == null) {
					return deniedPermission;
				}

				EntitySubdivisionForUserPermission subdivisionForUserPermission = PermissionRepository.GetSubdivisionForUserEntityPermission(uow, userId,  entityType.Name, employee.Subdivision.Id);
				if(subdivisionForUserPermission != null) {
					return new EntityPermission(
						subdivisionForUserPermission.CanCreate,
						subdivisionForUserPermission.CanRead,
						subdivisionForUserPermission.CanUpdate,
						subdivisionForUserPermission.CanDelete
					);
				}

				EntitySubdivisionPermission subdivisionPermission = PermissionRepository.GetSubdivisionEntityPermission(uow, entityType.Name, employee.Subdivision.Id);
				if(subdivisionPermission == null) {
					return deniedPermission;
				} else {
					return new EntityPermission(
						subdivisionPermission.CanCreate,
						subdivisionPermission.CanRead,
						subdivisionPermission.CanUpdate,
						subdivisionPermission.CanDelete
					);
				}
			}
		}
	}
}
