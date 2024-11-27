using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz;
using Vodovoz.EntityRepositories.Permissions;

namespace VodovozBusiness.Services.Subdivisions
{
	public interface ISubdivisionPermissionsService
	{
		IList<SubdivisionPermissionNode> AddSubdivisionEntityPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision);
		IList<SubdivisionPermissionNode> ReplaceSubdivisionEntityPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision);
	}
}
