using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Domain.Permissions;

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

		public static IList<EntitySubdivisionOnlyPermission> GetAllSubdivisionEntityPermissions(IUnitOfWork uow, int subdivisionId)
		{
			return uow.Session.QueryOver<EntitySubdivisionOnlyPermission>()
				.Where(x => x.Subdivision.Id == subdivisionId)
				.List();
		}

		public static IList<EntitySubdivisionForUserPermission> GetAllSubdivisionForUserEntityPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<EntitySubdivisionForUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();
		}
	}
}
