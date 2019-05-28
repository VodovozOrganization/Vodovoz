using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Permissions;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.EntityRepositories.Subdivisions
{
	public class SubdivisionRepository : ISubdivisionRepository
	{
		public IEnumerable<Subdivision> GetSubdivisionsForDocumentTypes(IUnitOfWork uow, Type[] documentTypes)
		{
			Subdivision subdivisionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver<Subdivision>(() => subdivisionAlias)
				.Left.JoinAlias(() => subdivisionAlias.DocumentTypes, () => typeOfEntityAlias)
				.WhereRestrictionOn(() => typeOfEntityAlias.Type).IsIn(documentTypes.Select(x => x.Name).ToArray())
				.List().Distinct();
		}

		public IEnumerable<Subdivision> GetCashSubdivisions(IUnitOfWork uow)
		{
			Type[] cashDocumentTypes = { typeof(Income), typeof(Expense), typeof(AdvanceReport) };
			return GetSubdivisionsForDocumentTypes(uow, cashDocumentTypes);
		}

		public IEnumerable<Subdivision> GetCashSubdivisionsAvailableForUser(IUnitOfWork uow, UserBase user)
		{
			Type[] cashDocumentTypes = { typeof(Income), typeof(Expense), typeof(AdvanceReport) };
			var validationResult = EntitySubdivisionForUserPermissionValidator.Validate(uow, user.Id, cashDocumentTypes);
			var subdivisionsList = new List<Subdivision>();
			foreach(var item in cashDocumentTypes) {
				subdivisionsList.AddRange(validationResult
					.Where(x => x.GetPermission(item).Read)
					.Select(x => x.Subdivision)
				);
			}
			return subdivisionsList.Distinct();
		}
	}
}
