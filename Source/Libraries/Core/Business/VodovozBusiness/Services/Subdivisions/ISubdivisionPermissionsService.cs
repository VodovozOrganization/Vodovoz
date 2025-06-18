using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace VodovozBusiness.Services.Subdivisions
{
	public interface ISubdivisionPermissionsService
	{
		IList<SubdivisionPermissionNode> AddSubdivisionEntityPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision);
		IList<SubdivisionPermissionNode> ReplaceSubdivisionEntityPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision);
		IList<SubdivisionWarehousePermission> AddWarehousePermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision);
		IList<SubdivisionWarehousePermission> ReplaceWarehousePermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision);
		IList<HierarchicalPresetSubdivisionPermission> AddPresetPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision);
		IList<HierarchicalPresetSubdivisionPermission> ReplacePresetPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision);
	}
}
