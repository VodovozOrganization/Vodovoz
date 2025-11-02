using System;
using System.Linq;
using System.Reflection;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Domain.Permissions
{
	public class EntityPermissionValidator : QS.DomainModel.Entity.EntityPermissions.EntityPermissionValidator
	{
		protected IEmployeeRepository employeeRepository;
		protected IPermissionRepository permissionRepository;
		private readonly IUnitOfWorkFactory _uowFactory;

		public EntityPermissionValidator(
			IEmployeeRepository employeeRepository, 
			IPermissionRepository permissionRepository,
			IUnitOfWorkFactory uowFactory
			) : base(uowFactory)
		{
			this.employeeRepository = employeeRepository ??
									  throw new ArgumentNullException(nameof(employeeRepository));
			this.permissionRepository = permissionRepository ??
						  throw new ArgumentNullException(nameof(permissionRepository));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public override EntityPermission Validate<TEntityType>(int userId)
		{
			return Validate(typeof(TEntityType), userId);
		}

		public override EntityPermission Validate(Type entityType, int userId)
		{
			var attribute = entityType.GetCustomAttribute<EntityPermissionAttribute>();
			if (attribute == null)
			{
				return EntityPermission.AllAllowed;
			}
			
			var permission = base.Validate(entityType, userId);
			if(!permission.IsEmpty) {
				return permission;
			}

			Employee employee;
			using(var uow = _uowFactory.CreateWithoutRoot()) {
				employee = employeeRepository.GetEmployeesForUser(uow, userId).FirstOrDefault();

				if(employee == null || employee.Subdivision == null) {
					return EntityPermission.AllDenied;
				}

				EntitySubdivisionForUserPermission subdivisionForUserPermission = permissionRepository.GetSubdivisionForUserEntityPermission(uow, userId,  entityType.Name, employee.Subdivision.Id);
				if(subdivisionForUserPermission != null) {
					return new EntityPermission(
						subdivisionForUserPermission.CanCreate,
						subdivisionForUserPermission.CanRead,
						subdivisionForUserPermission.CanUpdate,
						subdivisionForUserPermission.CanDelete
					);
				}

				var subdivision = employee.Subdivision;
				while(subdivision != null) {
					var subdivisionPermission = permissionRepository.GetSubdivisionEntityPermission(uow, entityType.Name, subdivision.Id);
					if(subdivisionPermission != null) {
						return new EntityPermission(
							subdivisionPermission.CanCreate,
							subdivisionPermission.CanRead,
							subdivisionPermission.CanUpdate,
							subdivisionPermission.CanDelete
						);
					}
					subdivision = subdivision.ParentSubdivision;
				}
				return EntityPermission.AllDenied;
			}
		}
	}
}
