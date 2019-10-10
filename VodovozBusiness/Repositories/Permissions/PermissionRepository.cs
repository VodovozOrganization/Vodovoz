using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Domain;
using QSSupportLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
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
			return uow.Session.QueryOver<EntitySubdivisionForUserPermission>(() => entitySubdivisionPermissionAlias)
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
			return uow.Session.QueryOver<EntitySubdivisionForUserPermission>(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => entitySubdivisionPermissionAlias.User.Id == userId)
				.WhereRestrictionOn(() => typeOfEntityAlias.Type).IsIn(entityNames)
				.List();
		}

		public static IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissionForOneEntity(IUnitOfWork uow, int userId, string entityName)
		{
			EntitySubdivisionForUserPermission entitySubdivisionPermissionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver<EntitySubdivisionForUserPermission>(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => entitySubdivisionPermissionAlias.User.Id == userId)
				.Where(() => typeOfEntityAlias.Type == entityName)
				.List();
		}

		public static IEnumerable<PermissionNode> GetAllSubdivisionEntityPermissions(IUnitOfWork uow, int subdivisionId, PermissionExtensionStore permissionExtensionFactory) //TODO : Вынести в фабрику (ну или в Helper) 
		{
			var basePermission = uow.Session.QueryOver<EntitySubdivisionOnlyPermission>()
				.Where(x => x.Subdivision.Id == subdivisionId)
				.List();

			foreach(var item in basePermission) {
				var node = new PermissionNode();
				node.EntitySubdivisionOnlyPermission = item;
				node.TypeOfEntity = item.TypeOfEntity;
				node.EntityPermissionExtended = new SortedList<string, EntityPermissionExtended>(StringComparer.Ordinal);
				foreach(var extension in permissionExtensionFactory.PermissionExtensions) 
				{
					Subdivision subdivisionAlias = null;
					User userAlias = null;
					EntityPermissionExtended permissionExtendedAlias = null;

					var permission = uow.Session.QueryOver(() => permissionExtendedAlias)
						.JoinAlias(x => x.User, () => userAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
						.JoinAlias(x => x.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
						.Where(x => subdivisionAlias.Id == subdivisionId)
						.And(Restrictions.On(() => userAlias).IsNull)
						.And(Restrictions.Eq(permissionExtendedAlias.PermissionId,extension.Key))
						.Take(1).List()?.FirstOrDefault();

					if(permission != null) {
						node.EntityPermissionExtended.Add(permission.PermissionId, permission);
						continue;
					}

					permission = new EntityPermissionExtended();
					permission.IsPermissionAvailable = false;
					permission.PermissionId = extension.Value.PermissionId;
					permission.Subdivision = item.Subdivision;
					permission.TypeOfEntity = item.TypeOfEntity;
					//node.EntitySubdivisionEntityPermissionWidgetPermissionExtended.Add(permission.PermissionId, permission);
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
			if(!MainSupport.BaseParameters.All.ContainsKey("accept_route_list_subdivision_restrict")) {
				throw new InvalidOperationException(String.Format("В базе не настроен параметр: accept_route_list_subdivision_restrict"));
			}
			int restrictSubdivision = int.Parse(MainSupport.BaseParameters.All["accept_route_list_subdivision_restrict"]);
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var userSubdivision = EmployeeRepository.GetEmployeeForCurrentUser(uow).Subdivision;
				if(userSubdivision == null) {
					return false;
				}
				return userSubdivision.Id != restrictSubdivision;
			}
		}
	}

	public class PermissionNode
	{
		public TypeOfEntity TypeOfEntity { get; set; }
		public EntitySubdivisionOnlyPermission EntitySubdivisionOnlyPermission { get; set; }
		public SortedList<string,EntityPermissionExtended> EntityPermissionExtended { get; set; }
	}
}
