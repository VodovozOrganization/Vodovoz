using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Service.BaseParametersServices;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Services;

namespace Vodovoz.Repositories.Permissions
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Permissions")]
	public static class PermissionRepository
	{
		public static EntitySubdivisionOnlyPermission GetSubdivisionEntityPermission(IUnitOfWork uow, string entityName, int subdisionId)
		{
			return new EntityRepositories.Permissions.PermissionRepository().GetSubdivisionEntityPermission(uow, entityName, subdisionId);
		}

		public static EntitySubdivisionForUserPermission GetSubdivisionForUserEntityPermission(IUnitOfWork uow, int userId, string entityName, int subdisionId)
		{
			return new EntityRepositories.Permissions.PermissionRepository().GetSubdivisionForUserEntityPermission(uow, userId, entityName, subdisionId);
		}

		public static IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForSomeEntities(IUnitOfWork uow, int userId, string[] entityNames)
		{
			return new EntityRepositories.Permissions.PermissionRepository().GetAllSubdivisionForUserEntityPermissionForSomeEntities(uow, userId, entityNames);
		}

		public static IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForOneEntity(IUnitOfWork uow, int userId, string entityName)
		{
			return new EntityRepositories.Permissions.PermissionRepository().GetAllSubdivisionForUserEntityPermissionForOneEntity(uow, userId, entityName);
		}

		public static IEnumerable<SubdivisionPermissionNode> GetAllSubdivisionEntityPermissions(IUnitOfWork uow, int subdivisionId, IPermissionExtensionStore permissionExtensionStore)
		{
			return new EntityRepositories.Permissions.PermissionRepository().GetAllSubdivisionEntityPermissions(uow, subdivisionId, permissionExtensionStore);
		}

		public static IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissions(IUnitOfWork uow, int userId)
		{
			return new EntityRepositories.Permissions.PermissionRepository().GetAllSubdivisionForUserEntityPermissions(uow, userId);
		}

		public static bool HasAccessToClosingRoutelist()
		{
			return new EntityRepositories.Permissions.PermissionRepository().HasAccessToClosingRoutelist(SubdivisionParametersProvider.Instance, EmployeeSingletonRepository.GetInstance());
		}
	}


}
