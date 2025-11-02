using System;
using System.Linq;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.PermissionExtensions
{
	public class EntityExtendedPermissionValidator : IEntityExtendedPermissionValidator
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public EntityExtendedPermissionValidator(IUnitOfWorkFactory uowFactory, IPermissionExtensionStore permissionExtensionStore, IEmployeeRepository employeeRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			PermissionExtensionStore = permissionExtensionStore ?? throw new NullReferenceException(nameof(permissionExtensionStore));
			EmployeeRepository = employeeRepository ?? throw new NullReferenceException(nameof(employeeRepository));
		}

		public IPermissionExtensionStore PermissionExtensionStore { get; }

		public IEmployeeRepository EmployeeRepository { get; }

		public bool Validate(Type entityType, int userId, string PermissionId)
		{
			return Validate(entityType, userId, PermissionExtensionStore.PermissionExtensions.FirstOrDefault(x => x.PermissionId == PermissionId));
		}

		public bool Validate(Type entityType, int userId, IPermissionExtension permissionExtension)
		{
			if(!permissionExtension.IsValidType(entityType))
				return false;
				
			using(var uow = _uowFactory.CreateWithoutRoot()) 
			{
				User user = uow.GetById<User>(userId);

				Employee employee = EmployeeRepository.GetEmployeesForUser(uow, userId).FirstOrDefault();

				TypeOfEntity typeOfEntity = uow.Session.QueryOver<TypeOfEntity>()
						.Where(x => x.Type == entityType.Name)
						.Take(1).List().FirstOrDefault();

				if(user == null)
					return false;
				if(user.IsAdmin)
					return true;
				if(typeOfEntity == null)
					return false;
				if(employee == null || employee.Subdivision == null)
					return false;
					
				var userPermission = uow.Session.QueryOver<EntityUserPermissionExtended>()
						.Where(x => x.User.Id == user.Id)
						.And(x => x.PermissionId == permissionExtension.PermissionId)
						.And(x => x.TypeOfEntity.Id == typeOfEntity.Id)
						.Take(1).List().FirstOrDefault();

				if(userPermission != null) {

					if(userPermission.IsPermissionAvailable == null)
						throw new NullReferenceException(nameof(userPermission));
					else
						return userPermission.IsPermissionAvailable.Value;

				}

				Subdivision subdivision = null;
				do {
					if(subdivision == null)
						subdivision = employee.Subdivision;
					else
						subdivision = subdivision.ParentSubdivision;
						
					var subdivisionPermission = uow.Session.QueryOver<EntitySubdivisionPermissionExtended>()
							.Where(x => x.Subdivision.Id == subdivision.Id)
							.And(x => x.PermissionId == permissionExtension.PermissionId)
							.And(x => x.TypeOfEntity.Id == typeOfEntity.Id)
							.Take(1).List().FirstOrDefault();

					if(subdivisionPermission == null)
						continue;

					if(subdivisionPermission.IsPermissionAvailable == null)
						throw new NullReferenceException(nameof(subdivisionPermission));

					return subdivisionPermission.IsPermissionAvailable.Value;

				}
				while(subdivision.ParentSubdivision != null);
			}
			return false;
		}
	}
}
