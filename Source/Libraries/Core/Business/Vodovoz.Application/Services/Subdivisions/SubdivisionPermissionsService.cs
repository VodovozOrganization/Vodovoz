using NHibernate.Util;
using System;
using System.Linq;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Application.Services.Subdivisions
{
	public class SubdivisionPermissionsService
	{
		private readonly IPermissionRepository _permissionRepository;

		public SubdivisionPermissionsService(
			IPermissionRepository permissionRepository)
		{
			_permissionRepository = permissionRepository ?? throw new System.ArgumentNullException(nameof(permissionRepository));
		}

		public void AddSubdiviionPermissions(Subdivision targer, Subdivision source)
		{
			if(targer.HasChildSubdivisions)
			{
				throw new InvalidOperationException("У целевого подразделения не должно быть дочерних подразделений");
			}
		}

		public void ReplaceSubdivisionPermissions(Subdivision targer, Subdivision source)
		{
			if(targer.HasChildSubdivisions)
			{
				throw new InvalidOperationException("У целевого подразделения не должно быть дочерних подразделений");
			}
		}
	}
}
