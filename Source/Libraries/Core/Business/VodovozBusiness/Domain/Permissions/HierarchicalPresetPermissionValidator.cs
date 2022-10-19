using System;
using System.Linq;
using QS.DomainModel.Entity.PresetPermissions;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Domain.Permissions
{
	public class HierarchicalPresetPermissionValidator : IPresetPermissionValidator
	{
		protected IEmployeeRepository employeeRepository;

		protected IPermissionRepository permissionRepository;

		public HierarchicalPresetPermissionValidator(IEmployeeRepository employeeRepository, IPermissionRepository permissionRepository)
		{
			this.employeeRepository = employeeRepository ??
									  throw new ArgumentNullException(nameof(employeeRepository));
			this.permissionRepository = permissionRepository ??
						  throw new ArgumentNullException(nameof(permissionRepository));
		}

		public bool Validate(string presetPermissionName, int userId)
		{
			if(String.IsNullOrEmpty(presetPermissionName))
				return false;
			if(userId == default(int))
				return false;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) 
			{
				var user = uow.GetById<User>(userId);

				if(user.IsAdmin)
					return true;

				HierarchicalPresetUserPermission perm;
				perm = permissionRepository.GetPresetUserPermission(uow, user, presetPermissionName);

				if(perm != null)
					return perm.Value;

				var employee = employeeRepository.GetEmployeesForUser(uow, userId).FirstOrDefault();

				if(employee == null || employee.Subdivision == null)
					return false;

				var subdivision = employee.Subdivision;

				while(subdivision != null) {
					var subdivisionPermission = permissionRepository.GetPresetSubdivisionPermission(uow, subdivision, presetPermissionName);
					if(subdivisionPermission != null)
						return subdivisionPermission.Value;

					subdivision = subdivision.ParentSubdivision;
				}
			}
			return false;
		}
	}
}
