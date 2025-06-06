using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.Infrastructure.Persistance.Permissions
{
	internal sealed class PermissionRepository : IPermissionRepository
	{
		public IEnumerable<SubdivisionPermissionNode> GetAllSubdivisionEntityPermissions(IUnitOfWork uow, int subdivisionId, IPermissionExtensionStore permissionExtensionStore)
		{
			var basePermission = uow.Session.QueryOver<EntitySubdivisionOnlyPermission>()
				.Where(x => x.Subdivision.Id == subdivisionId)
				.List();

			foreach(var item in basePermission)
			{
				var node = new SubdivisionPermissionNode();
				node.EntitySubdivisionOnlyPermission = item;
				node.TypeOfEntity = item.TypeOfEntity;
				node.EntityPermissionExtended = new List<EntitySubdivisionPermissionExtended>();
				foreach(var extension in permissionExtensionStore.PermissionExtensions)
				{
					EntitySubdivisionPermissionExtended permissionExtendedAlias = null;

					var permission = uow.Session.QueryOver(() => permissionExtendedAlias)
						.Where(x => x.Subdivision.Id == subdivisionId)
						.And(() => permissionExtendedAlias.PermissionId == extension.PermissionId)
						.And(x => x.TypeOfEntity.Id == node.TypeOfEntity.Id)
						.Take(1)?.List()?.FirstOrDefault();

					if(permission != null)
					{
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

		public bool HasAccessToClosingRoutelist(IUnitOfWork uow, ISubdivisionRepository subdivisionRepository, IEmployeeRepository employeeRepository, IUserService userService)
		{
			return userService.GetCurrentUser().IsAdmin
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

		public IList<HierarchicalPresetSubdivisionPermission> GetAllPresetPermissionsBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			return uow.Session.QueryOver<HierarchicalPresetSubdivisionPermission>()
				.Where(x => x.Subdivision.Id == subdivisionId)
				.List();
		}

		public IList<SubdivisionWarehousePermission> GetAllWarehousePermissionsBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			return uow.Session
				.QueryOver<SubdivisionWarehousePermission>().Where(x => x.Subdivision.Id == subdivisionId)
				.List();
		}

		public HierarchicalPresetUserPermission GetPresetUserPermission(IUnitOfWork uow, User user, string permission)
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

		public IList<UserNode> GetUsersWithActivePresetPermission(IUnitOfWork uow, string permissionName)
		{
			Subdivision subdivisionAlias = null;
			User userAlias = null;
			HierarchicalPresetUserPermission presetUserPermissionAlias = null;
			UserNode resultAlias = null;

			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.JoinAlias(e => e.Subdivision, () => subdivisionAlias)
				.JoinEntityAlias(() => presetUserPermissionAlias, () => userAlias.Id == presetUserPermissionAlias.User.Id)
				.Where(() => presetUserPermissionAlias.PermissionName == permissionName)
				.And(() => presetUserPermissionAlias.Value)
				.SelectList(list => list
					.Select(() => userAlias.Id).WithAlias(() => resultAlias.UserId)
					.Select(() => userAlias.Name).WithAlias(() => resultAlias.UserName)
					.Select(() => userAlias.Deactivated).WithAlias(() => resultAlias.IsDeactivatedUser)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.UserSubdivision))
				.OrderBy(() => userAlias.Deactivated).Asc
				.ThenBy(() => subdivisionAlias.Name).Asc
				.ThenBy(() => userAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<UserNode>())
				.List<UserNode>();
		}

		public IList<UserPresetPermissionWithSubdivisionNode> GetUsersWithSubdivisionsPresetPermission(IUnitOfWork uow)
		{
			return GetUsersWithSubdivisions()
				.GetExecutableQueryOver(uow.Session)
				.TransformUsing(Transformers.AliasToBean<UserPresetPermissionWithSubdivisionNode>())
				.List<UserPresetPermissionWithSubdivisionNode>();
		}

		public IList<UserEntityExtendedPermissionWithSubdivisionNode> GetUsersWithSubdivisionsEntityPermission(IUnitOfWork uow)
		{
			return GetUsersWithSubdivisions()
				.GetExecutableQueryOver(uow.Session)
				.TransformUsing(Transformers.AliasToBean<UserEntityExtendedPermissionWithSubdivisionNode>())
				.List<UserEntityExtendedPermissionWithSubdivisionNode>();
		}

		public IList<UserEntityExtendedPermissionNode> GetUsersEntityPermission(IUnitOfWork uow, string permissionName)
		{
			Subdivision subdivisionAlias = null;
			User userAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			EntityUserPermission entityUserPermissionAlias = null;
			EntityUserPermissionExtended entityUserPermissionExtendedAlias = null;
			UserEntityExtendedPermissionNode resultAlias = null;

			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.Subdivision, () => subdivisionAlias)
				.JoinAlias(e => e.User, () => userAlias)
				.JoinEntityAlias(() => entityUserPermissionAlias, () => userAlias.Id == entityUserPermissionAlias.User.Id)
				.JoinAlias(() => entityUserPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.JoinEntityAlias(() => entityUserPermissionExtendedAlias,
					() => entityUserPermissionExtendedAlias.User.Id == userAlias.Id
					&& entityUserPermissionExtendedAlias.TypeOfEntity.Id == typeOfEntityAlias.Id,
					JoinType.LeftOuterJoin)
				.Where(() => typeOfEntityAlias.Type == permissionName)
				.SelectList(list => list
					.Select(() => userAlias.Id).WithAlias(() => resultAlias.UserId)
					.Select(() => userAlias.Name).WithAlias(() => resultAlias.UserName)
					.Select(() => userAlias.Deactivated).WithAlias(() => resultAlias.IsDeactivatedUser)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.UserSubdivision)
					.Select(() => entityUserPermissionAlias.CanRead).WithAlias(() => resultAlias.CanRead)
					.Select(() => entityUserPermissionAlias.CanCreate).WithAlias(() => resultAlias.CanCreate)
					.Select(() => entityUserPermissionAlias.CanUpdate).WithAlias(() => resultAlias.CanUpdate)
					.Select(() => entityUserPermissionAlias.CanDelete).WithAlias(() => resultAlias.CanDelete)
					.Select(() => entityUserPermissionExtendedAlias.IsPermissionAvailable)
						.WithAlias(() => resultAlias.ExtendedPermissionValue))
				.OrderBy(() => userAlias.Deactivated).Asc
				.ThenBy(() => subdivisionAlias.Name).Asc
				.ThenBy(() => userAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<UserEntityExtendedPermissionNode>())
				.List<UserEntityExtendedPermissionNode>();
		}

		public EntityExtendedPermission GetSubdivisionEntityExtendedPermission(
			IUnitOfWork uow, int subdivisionId, string permissionName)
		{
			TypeOfEntity typeOfEntityAlias = null;
			EntitySubdivisionOnlyPermission entitySubdivisionPermissionAlias = null;
			EntitySubdivisionPermissionExtended entitySubdivisionPermissionExtendedAlias = null;
			EntityExtendedPermission resultAlias = null;

			return uow.Session.QueryOver(() => entitySubdivisionPermissionAlias)
				.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.JoinEntityAlias(() => entitySubdivisionPermissionExtendedAlias,
					() => entitySubdivisionPermissionExtendedAlias.Subdivision.Id == entitySubdivisionPermissionAlias.Subdivision.Id
					&& entitySubdivisionPermissionExtendedAlias.TypeOfEntity.Id == typeOfEntityAlias.Id,
					JoinType.LeftOuterJoin)
				.Where(() => entitySubdivisionPermissionAlias.Subdivision.Id == subdivisionId)
				.Where(() => typeOfEntityAlias.Type == permissionName)
				.SelectList(list => list
					.Select(() => entitySubdivisionPermissionAlias.CanRead).WithAlias(() => resultAlias.CanRead)
					.Select(() => entitySubdivisionPermissionAlias.CanCreate).WithAlias(() => resultAlias.CanCreate)
					.Select(() => entitySubdivisionPermissionAlias.CanUpdate).WithAlias(() => resultAlias.CanUpdate)
					.Select(() => entitySubdivisionPermissionAlias.CanDelete).WithAlias(() => resultAlias.CanDelete)
					.Select(() => entitySubdivisionPermissionExtendedAlias.IsPermissionAvailable)
						.WithAlias(() => resultAlias.ExtendedPermissionValue))
				.TransformUsing(Transformers.AliasToBean<EntityExtendedPermission>())
				.SingleOrDefault<EntityExtendedPermission>();
		}

		private QueryOver<HierarchicalPresetUserPermission, HierarchicalPresetUserPermission> GetPresetUserPermissionByUserId(int userId)
		{
			return QueryOver.Of<HierarchicalPresetUserPermission>()
				.Where(x => x.User.Id == userId);
		}

		private QueryOver<Employee, Employee> GetUsersWithSubdivisions()
		{
			Employee employeeAlias = null;
			UserBase userAlias = null;
			UserWithSubdivisionNode resultAlias = null;
			Subdivision subdivisionAlias = null;

			return QueryOver.Of(() => employeeAlias)
				.JoinAlias(() => employeeAlias.User, () => userAlias)
				.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionAlias)
				.SelectList(list => list
					.Select(Projections.Entity<UserBase>(nameof(userAlias))).WithAlias(() => resultAlias.User)
					.Select(Projections.Entity<Subdivision>(nameof(subdivisionAlias))).WithAlias(() => resultAlias.Subdivision))
				.OrderBy(() => userAlias.Deactivated).Asc
				.ThenBy(() => subdivisionAlias.Name).Asc
				.ThenBy(() => userAlias.Name).Asc;
		}
	}
}
