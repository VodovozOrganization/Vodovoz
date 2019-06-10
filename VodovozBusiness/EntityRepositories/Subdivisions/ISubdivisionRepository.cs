using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;
namespace Vodovoz.EntityRepositories.Subdivisions
{
	public interface ISubdivisionRepository
	{
		IEnumerable<Subdivision> GetSubdivisionsForDocumentTypes(IUnitOfWork uow, Type[] documentTypes);
		IEnumerable<Subdivision> GetCashSubdivisions(IUnitOfWork uow);
		IEnumerable<Subdivision> GetCashSubdivisionsAvailableForUser(IUnitOfWork uow, UserBase user);
	}
}
