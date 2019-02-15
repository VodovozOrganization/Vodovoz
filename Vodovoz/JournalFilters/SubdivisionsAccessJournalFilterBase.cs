using System;
using NHibernate;
using QSOrmProject.RepresentationModel;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Permissions;
using System.Linq;
using QS.DomainModel.Entity;
using System.Collections.Generic;

namespace Vodovoz.JournalFilters
{
	public class SubdivisionsAccessJournalFilterBase<TFilter> : RepresentationFilterBase<TFilter>, ISubdivisionAccessFilter
		where TFilter : class
	{
		protected virtual IEnumerable<Subdivision> AllowedSubdivisions { get; set; }
		protected virtual bool ShowSubdivisions { get; set; }
		private IEnumerable<Subdivision> mainSubdivisions;
		protected virtual IEnumerable<Subdivision> Subdivisions => mainSubdivisions;

		public SubdivisionsAccessJournalFilterBase()
		{
		}

		public void InitSubdivisionsAccess(Type[] types)
		{
			var validationResult = EntitySubdivisionForUserPermissionValidator.Validate(UoW, types);

			var subdivisionsList = new List<Subdivision>();
			foreach(var item in types) {
				subdivisionsList.AddRange(validationResult
					.Where(x => x.GetPermission(item).Read)
					.Select(x => x.Subdivision)
				);
			}
			mainSubdivisions = validationResult.Where(x => x.IsMainSubdivision).Select(x => x.Subdivision);
			ShowSubdivisions = validationResult.Any(x => !x.IsMainSubdivision) && subdivisionsList.Any();
			AllowedSubdivisions = subdivisionsList;
		}

		public IQueryOver<TEntity, TEntity> FilterBySubdivisionsAccess<TEntity>(IQueryOver<TEntity, TEntity> baseQuery)
			where TEntity : class, IDomainObject, ISubdivisionEntity
		{
			return baseQuery.WhereRestrictionOn(x => x.RelatedToSubdivision.Id).IsIn(Subdivisions.Select(x => x.Id).ToArray());
		}
	}
}
