using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Domain;
using QSSupportLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.Repositories.Permissions
{
	public static class PermissionRepository
	{
		public static EntitySubdivisionOnlyPermission GetSubdivisionEntityPermission(IUnitOfWork uow, string entityName, int subdisionId)
		{
			EntitySubdivisionOnlyPermission entitySubdivisionPermissionAlias = null;
			Subdivision subdivisionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver<EntitySubdivisionOnlyPermission>(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => subdivisionAlias.Id == subdisionId)
				.Where(() => typeOfEntityAlias.Type == entityName)
				.SingleOrDefault();
		}

		public static EntitySubdivisionForUserPermission GetSubdivisionForUserEntityPermission(IUnitOfWork uow, int userId, string entityName, int subdisionId)
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

		public static IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForSomeEntities(IUnitOfWork uow, int userId, string[] entityNames)
		{
			EntitySubdivisionForUserPermission entitySubdivisionPermissionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => entitySubdivisionPermissionAlias.User.Id == userId)
				.WhereRestrictionOn(() => typeOfEntityAlias.Type).IsIn(entityNames)
				.List();
		}

		public static IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForOneEntity(IUnitOfWork uow, int userId, string entityName)
		{
			EntitySubdivisionForUserPermission entitySubdivisionPermissionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => entitySubdivisionPermissionAlias.User.Id == userId)
				.Where(() => typeOfEntityAlias.Type == entityName)
				.List();
		}

		public static IEnumerable<SubdivisionPermissionNode> GetAllSubdivisionEntityPermissions(IUnitOfWork uow, int subdivisionId, IPermissionExtensionStore permissionExtensionStore)
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

		public static IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<EntitySubdivisionForUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();
		}

		public static bool HasAccessToClosingRoutelist()
		{
			//FIXME исправить на нормальную проверку права этого подразделения
			//необходимо правильно хранить подразделения которым запрещен доступ к опредленным функциям системы
			if(!ParametersProvider.Instance.ContainsParameter("accept_route_list_subdivision_restrict")) {
				throw new InvalidOperationException(String.Format("В базе не настроен параметр: accept_route_list_subdivision_restrict"));
			}
			int restrictSubdivision = int.Parse(ParametersProvider.Instance.GetParameterValue("accept_route_list_subdivision_restrict"));
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var userSubdivision = EmployeeRepository.GetEmployeeForCurrentUser(uow).Subdivision;
				if(userSubdivision == null) {
					return false;
				}
				return userSubdivision.Id != restrictSubdivision;
			}
		}
	}

	public class SubdivisionPermissionNode : IPermissionNode
	{
		public TypeOfEntity TypeOfEntity { get; set; }
		public EntitySubdivisionOnlyPermission EntitySubdivisionOnlyPermission { get; set; }
		public IList<EntitySubdivisionPermissionExtended> EntityPermissionExtended { get; set; }

		public EntityPermissionBase EntityPermission => EntitySubdivisionOnlyPermission;
		IList<EntityPermissionExtendedBase> IPermissionNode.EntityPermissionExtended 
		{ 
			get => EntityPermissionExtended.OfType<EntityPermissionExtendedBase>().ToList();
			set => EntityPermissionExtended = value.OfType<EntitySubdivisionPermissionExtended>().ToList();
		}
	}
}
