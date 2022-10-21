using System;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repositories.Permissions;

namespace Vodovoz.Domain.Permissions
{
	public class EntityExtendedPermissionValidator
	{
		public EntityExtendedPermissionValidator(IPermissionExtensionStore permissionExtensionStore, IEmployeeRepository employeeRepository, IUserRepository userRepository)
		{
			PermissionExtensionStore = permissionExtensionStore ?? throw new NullReferenceException(nameof(permissionExtensionStore));
			EmployeeRepository = employeeRepository ?? throw new NullReferenceException(nameof(employeeRepository));
			UserRepository = userRepository ?? throw new NullReferenceException(nameof(userRepository));
		}

		public IPermissionExtensionStore PermissionExtensionStore { get; }

		public IEmployeeRepository EmployeeRepository { get; }

		public IUserRepository UserRepository { get; }

		public bool Validate(Type entityType, int userId, string PermissionId)
		{
			return Validate(entityType, userId, PermissionExtensionStore.PermissionExtensions.FirstOrDefault(x => x.PermissionId == PermissionId));
		}

		public bool Validate(Type entityType, int userId, IPermissionExtension permissionExtension)
		{
			if(!permissionExtension.IsValidType(entityType))
				return false;
				
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) 
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

				Subdivision subdivisionAlias = null;
				var userPermission = uow.Session.QueryOver<EntityPermissionExtended>()
						.Left.JoinAlias(x => x.Subdivision,() => subdivisionAlias)
						.Where(x => x.User.Id == user.Id)
						.And(Restrictions.On(() => subdivisionAlias.Id).IsNull)
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
					if(subdivisionAlias == null)
						subdivisionAlias = employee.Subdivision;
					else
						subdivisionAlias = subdivisionAlias.ParentSubdivision;

					User userAlias = null;
					var subdivisionPermission = uow.Session.QueryOver<EntityPermissionExtended>()
						.Left.JoinAlias(x => x.User, () => userAlias)
							.Where(x => x.Subdivision.Id == subdivisionAlias.Id)
							.And(Restrictions.On(() => userAlias.Id).IsNull)
							.And(x => x.PermissionId == permissionExtension.PermissionId)
							.And(x => x.TypeOfEntity.Id == typeOfEntity.Id)
							.Take(1).List().FirstOrDefault();

					if(subdivisionPermission == null)
						continue;

					if(subdivisionPermission.IsPermissionAvailable == null)
						throw new NullReferenceException(nameof(subdivisionPermission));

					return subdivisionPermission.IsPermissionAvailable.Value;

				}
				while(subdivisionAlias.ParentSubdivision != null);
			}
			return false;
		}
	}
}