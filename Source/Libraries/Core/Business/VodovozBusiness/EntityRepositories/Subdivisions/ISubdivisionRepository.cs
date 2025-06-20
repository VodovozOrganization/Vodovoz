using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Subdivisions
{
	public interface ISubdivisionRepository
	{
		IEnumerable<Subdivision> GetSubdivisionsForDocumentTypes(IUnitOfWork uow, Type[] documentTypes);
		IEnumerable<Subdivision> GetCashSubdivisions(IUnitOfWork uow);
		IEnumerable<Subdivision> GetCashSubdivisionsAvailableForUser(IUnitOfWork uow, UserBase user);

		IList<Subdivision> GetAllDepartmentsOrderedByName(IUnitOfWork uow, bool orderByDescending = false);
		IEnumerable<Subdivision> GetAvailableSubdivionsForUser(IUnitOfWork uow, Type[] documentsTypes);
		IList<Subdivision> GetChildDepartments(IUnitOfWork uow, Subdivision parentSubdivision, bool orderByDescending = false);
		Subdivision GetQCDepartment(IUnitOfWork uow);
		IList<Warehouse> GetWarehouses(IUnitOfWork uow, Subdivision subdivision, bool orderByDescending = false);
		IEnumerable<int> GetAllSubdivisionsIds(IUnitOfWork uow);
		IList<NamedDomainObjectNode> GetAvailableSubdivisionsInAccordingWithCarTypeAndOwner(
			IUnitOfWork uow, CarTypeOfUse[] carTypeOfUses, CarOwnType[] carOwnTypes);
	}
}
