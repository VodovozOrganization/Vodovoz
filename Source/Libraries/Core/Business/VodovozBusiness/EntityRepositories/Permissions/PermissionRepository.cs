using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.EntityRepositories.Permissions
{
	public class PermissionRepository : IPermissionRepository
	{
		public IEnumerable<SubdivisionPermissionNode> GetAllSubdivisionEntityPermissions(IUnitOfWork uow, int subdivisionId, IPermissionExtensionStore permissionExtensionStore)
		{
			var basePermission = uow.Session.QueryOver<EntitySubdivisionOnlyPermission>()
				.Where(x => x.Subdivision.Id == subdivisionId)
				.List();

			foreach(var item in basePermission) {
				var node = new SubdivisionPermissionNode();
				node.EntitySubdivisionOnlyPermission = item;
				node.TypeOfEntity = item.TypeOfEntity;
				node.EntityPermissionExtended = new List<EntitySubdivisionPermissionExtended>();
				foreach(var extension in permissionExtensionStore.PermissionExtensions) {
					EntitySubdivisionPermissionExtended permissionExtendedAlias = null;

					var permission = uow.Session.QueryOver(() => permissionExtendedAlias)
						.Where(x => x.Subdivision.Id == subdivisionId)
						.And(() => permissionExtendedAlias.PermissionId == extension.PermissionId)
						.And(x => x.TypeOfEntity.Id == node.TypeOfEntity.Id)
						.Take(1)?.List()?.FirstOrDefault();

					if(permission != null) {
						node.EntityPermissionExtended.Add(permission);
						continue;
					}

					permission = new EntitySubdivisionPermissionExtended();
					permission.IsPermissionAvailable = null;
					permission.PermissionId = extension.PermissionId;
					permission.Subdivision = item.Subdivision;
					permission.TypeOfEntity = item.TypeOfEntity;
					node.EntityPermissionExtended.Add(permission);
				}

				yield return node;
			}
		}

		public IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForOneEntity(IUnitOfWork uow, int userId, string entityName)
		{
			EntitySubdivisionForUserPermission entitySubdivisionPermissionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => entitySubdivisionPermissionAlias.User.Id == userId)
				.Where(() => typeOfEntityAlias.Type == entityName)
				.List();
		}

		public IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForSomeEntities(IUnitOfWork uow, int userId, string[] entityNames)
		{
			EntitySubdivisionForUserPermission entitySubdivisionPermissionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => entitySubdivisionPermissionAlias.User.Id == userId)
				.WhereRestrictionOn(() => typeOfEntityAlias.Type).IsIn(entityNames)
				.List();
		}

		public IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<EntitySubdivisionForUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();
		}

		public EntitySubdivisionOnlyPermission GetSubdivisionEntityPermission(IUnitOfWork uow, string entityName, int subdisionId)
		{
			EntitySubdivisionOnlyPermission entitySubdivisionPermissionAlias = null;
			Subdivision subdivisionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => subdivisionAlias.Id == subdisionId)
				.Where(() => typeOfEntityAlias.Type == entityName)
				.SingleOrDefault();
		}

		public EntitySubdivisionForUserPermission GetSubdivisionForUserEntityPermission(IUnitOfWork uow, int userId, string entityName, int subdisionId)
		{
			EntitySubdivisionForUserPermission entitySubdivisionPermissionAlias = null;
			Subdivision subdivisionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => entitySubdivisionPermissionAlias.Subdivision.Id == subdisionId)
				.Where(() => entitySubdivisionPermissionAlias.User.Id == userId)
				.Where(() => typeOfEntityAlias.Type == entityName)
				.SingleOrDefault();
		}

		public bool HasAccessToClosingRoutelist(IUnitOfWork uow, ISubdivisionRepository subdivisionRepository , IEmployeeRepository employeeRepository, IUserService userService)
		{
			return userService.GetCurrentUser(uow).IsAdmin
				|| subdivisionRepository.GetCashSubdivisions(uow).Contains(employeeRepository.GetEmployeeForCurrentUser(uow).Subdivision);
		}

		public HierarchicalPresetSubdivisionPermission GetPresetSubdivisionPermission(IUnitOfWork uow, Subdivision subdivision, string permission)
		{
			return uow.Session.QueryOver<HierarchicalPresetSubdivisionPermission>()
					.Where(x => x.Subdivision.Id == subdivision.Id && x.PermissionName == permission)
					.SingleOrDefault();
		}

		public IList<HierarchicalPresetSubdivisionPermission> GetAllPresetSubdivisionPermission(IUnitOfWork uow, Subdivision subdivision)
		{
			return uow.Session.QueryOver<HierarchicalPresetSubdivisionPermission>()
						.Where(x => x.Subdivision.Id == subdivision.Id)
						.List();
		}

		public HierarchicalPresetUserPermission GetPresetUserPermission(IUnitOfWork uow, Domain.Employees.User user, string permission)
		{
			return uow.Session.QueryOver<HierarchicalPresetUserPermission>()
					.Where(x => x.User.Id == user.Id && x.PermissionName == permission)
					.SingleOrDefault();
		}

		public IList<HierarchicalPresetUserPermission> GetAllPresetUserPermission(IUnitOfWork uow, int userId)
		{
			return GetPresetUserPermissionByUserId(userId)
				.GetExecutableQueryOver(uow.Session)
				.List();
		}
		
		public IList<HierarchicalPresetPermissionBase> GetAllPresetUserPermissionBase(IUnitOfWork uow, int userId)
		{
			return GetPresetUserPermissionByUserId(userId)
				.GetExecutableQueryOver(uow.Session)
				.List<HierarchicalPresetPermissionBase>();
		}

		public IList<WarehousePermissionBase> GetAllUserWarehousesPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<UserWarehousePermission>()
				.Where(x => x.User.Id == userId)
				.List<WarehousePermissionBase>();
		}
		
		public IEnumerable<EntityUserPermission> GetAllEntityUserPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<EntityUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();
		}
		
		public IEnumerable<EntityUserPermissionExtended> GetAllEntityUserPermissionsExtended(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<EntityUserPermissionExtended>()
				.Where(x => x.User.Id == userId)
				.List();
		}

		public UserWarehousePermission GetUserWarehousePermission(
			IUnitOfWork uow, int userId, int warehouseId, WarehousePermissionsType warehousePermissionsType)
		{
			return uow.Session.QueryOver<UserWarehousePermission>()
				.Where(x => x.User.Id == userId)
				.And(x => x.Warehouse.Id == warehouseId)
				.And(x => x.WarehousePermissionType == warehousePermissionsType)
				.SingleOrDefault();
		}
		
		public SubdivisionWarehousePermission GetSubdivisionWarehousePermission(
			IUnitOfWork uow, int subdivisionId, int warehouseId, WarehousePermissionsType warehousePermissionsType)
		{
			return uow.Session.QueryOver<SubdivisionWarehousePermission>()
				.Where(x => x.Subdivision.Id == subdivisionId)
				.And(x => x.Warehouse.Id == warehouseId)
				.And(x => x.WarehousePermissionType == warehousePermissionsType)
				.SingleOrDefault();
		}
		
		private QueryOver<HierarchicalPresetUserPermission, HierarchicalPresetUserPermission> GetPresetUserPermissionByUserId(int userId)
		{
			return QueryOver.Of<HierarchicalPresetUserPermission>()
				.Where(x => x.User.Id == userId);
		}
	}
}
