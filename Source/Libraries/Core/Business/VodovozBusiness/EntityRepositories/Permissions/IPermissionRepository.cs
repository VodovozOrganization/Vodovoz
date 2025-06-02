using System.Collections.Generic;
using FluentNHibernate.Conventions.Inspections;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.EntityRepositories.Permissions
{
	public interface IPermissionRepository
	{
		EntitySubdivisionOnlyPermission GetSubdivisionEntityPermission(IUnitOfWork uow, string entityName, int subdisionId);
		EntitySubdivisionForUserPermission GetSubdivisionForUserEntityPermission(IUnitOfWork uow, int userId, string entityName, int subdisionId);
		IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForSomeEntities(IUnitOfWork uow, int userId, string[] entityNames);
		IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForOneEntity(IUnitOfWork uow, int userId, string entityName);
		IEnumerable<SubdivisionPermissionNode> GetAllSubdivisionEntityPermissions(IUnitOfWork uow, int subdivisionId, IPermissionExtensionStore permissionExtensionStore);
		IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissions(IUnitOfWork uow, int userId);
		bool HasAccessToClosingRoutelist(IUnitOfWork uow, ISubdivisionRepository subdivisionRepository , IEmployeeRepository employeeRepository, IUserService userService);
		HierarchicalPresetUserPermission GetPresetUserPermission(IUnitOfWork uow, User user, string permission);
		IList<HierarchicalPresetUserPermission> GetAllPresetUserPermission(IUnitOfWork uow, int userId);
		IList<HierarchicalPresetPermissionBase> GetAllPresetUserPermissionBase(IUnitOfWork uow, int userId);
		HierarchicalPresetSubdivisionPermission GetPresetSubdivisionPermission(IUnitOfWork uow, Subdivision subdivision, string permission);
		IList<HierarchicalPresetSubdivisionPermission> GetAllPresetSubdivisionPermission(IUnitOfWork uow, Subdivision subdivision);
		IList<HierarchicalPresetSubdivisionPermission> GetAllPresetPermissionsBySubdivision(IUnitOfWork uow, int subdivisionId);
		IList<SubdivisionWarehousePermission> GetAllWarehousePermissionsBySubdivision(IUnitOfWork uow, int subdivisionId);
		IList<WarehousePermissionBase> GetAllUserWarehousesPermissions(IUnitOfWork uow, int userId);
		IEnumerable<EntityUserPermission> GetAllEntityUserPermissions(IUnitOfWork uow, int userId);
		IEnumerable<EntityUserPermissionExtended> GetAllEntityUserPermissionsExtended(IUnitOfWork uow, int userId);
		UserWarehousePermission GetUserWarehousePermission(
			IUnitOfWork uow, int userId, int warehouseId, WarehousePermissionsType warehousePermissionsType);
		SubdivisionWarehousePermission GetSubdivisionWarehousePermission(
			IUnitOfWork uow, int subdivisionId, int warehouseId, WarehousePermissionsType warehousePermissionsType);
		IList<UserNode> GetUsersWithActivePresetPermission(IUnitOfWork uow, string permissionName);
		IList<UserPresetPermissionWithSubdivisionNode> GetUsersWithSubdivisionsPresetPermission(IUnitOfWork uow);
		IList<UserEntityExtendedPermissionWithSubdivisionNode> GetUsersWithSubdivisionsEntityPermission(IUnitOfWork uow);
		IList<UserEntityExtendedPermissionNode> GetUsersEntityPermission(IUnitOfWork uow, string permissionName);
		EntityExtendedPermission GetSubdivisionEntityExtendedPermission(IUnitOfWork uow, int subdivisionId, string permissionName);
	}
}
