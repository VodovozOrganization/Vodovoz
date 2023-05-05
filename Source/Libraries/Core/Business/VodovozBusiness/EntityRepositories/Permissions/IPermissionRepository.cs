﻿using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
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
		HierarchicalPresetUserPermission GetPresetUserPermission(IUnitOfWork uow, Domain.Employees.User user, string permission);
		IList<HierarchicalPresetUserPermission> GetAllPresetUserPermission(IUnitOfWork uow, int userId);
		IList<HierarchicalPresetPermissionBase> GetAllPresetUserPermissionBase(IUnitOfWork uow, int userId);
		HierarchicalPresetSubdivisionPermission GetPresetSubdivisionPermission(IUnitOfWork uow, Subdivision subdivision, string permission);
		IList<HierarchicalPresetSubdivisionPermission> GetAllPresetSubdivisionPermission(IUnitOfWork uow, Subdivision subdivision);
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

	public class SubdivisionPermissionNode : IPermissionNode
	{
		public TypeOfEntity TypeOfEntity { get; set; }
		public EntitySubdivisionOnlyPermission EntitySubdivisionOnlyPermission { get; set; }
		public IList<EntitySubdivisionPermissionExtended> EntityPermissionExtended { get; set; }

		public EntityPermissionBase EntityPermission => EntitySubdivisionOnlyPermission;
		IList<EntityPermissionExtendedBase> IPermissionNode.EntityPermissionExtended {
			get => EntityPermissionExtended.OfType<EntityPermissionExtendedBase>().ToList();
			set => EntityPermissionExtended = value.OfType<EntitySubdivisionPermissionExtended>().ToList();
		}
	}
}
