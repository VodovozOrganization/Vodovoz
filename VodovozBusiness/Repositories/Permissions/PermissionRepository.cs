using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Repositories.Permissions
{
	public static class PermissionRepository
	{
		public static EntitySubdivisionPermission GetSubdivisionEntityPermission(IUnitOfWork uow, string entityName, int subdisionId)
		{
			TypeOfEntity typeOfEntityAlias = null;
			EntitySubdivisionPermission entitySubdivisionPermissionAlias = null;
			Subdivision subdivisionAlias = null;
			return uow.Session.QueryOver<EntitySubdivisionPermission>(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => subdivisionAlias.Id == subdisionId)
				.Where(() => typeOfEntityAlias.Type == entityName)
				.SingleOrDefault();
		}

		public static IList<EntitySubdivisionPermission> GetAllSubdivisionEntityPermissions(IUnitOfWork uow, int subdisionId)
		{
			TypeOfEntity typeOfEntityAlias = null;
			EntitySubdivisionPermission entitySubdivisionPermissionAlias = null;
			Subdivision subdivisionAlias = null;
			return uow.Session.QueryOver<EntitySubdivisionPermission>(() => entitySubdivisionPermissionAlias)
				.Left.JoinAlias(() => entitySubdivisionPermissionAlias.Subdivision, () => subdivisionAlias)
				.Where(() => subdivisionAlias.Id == subdisionId)
				.List();
		}
	}
}
