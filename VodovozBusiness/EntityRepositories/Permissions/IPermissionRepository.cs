using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Services;

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

		IList<HierarchicalPresetUserPermission> GetAllPresetUserPermission(IUnitOfWork uow, Domain.Employees.User user);

		HierarchicalPresetSubdivisionPermission GetPresetSubdivisionPermission(IUnitOfWork uow, Subdivision subdivision, string permission);

		IList<HierarchicalPresetSubdivisionPermission> GetAllPresetSubdivisionPermission(IUnitOfWork uow, Subdivision subdivision);
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
