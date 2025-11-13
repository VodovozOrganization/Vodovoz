using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Core.Domain.Repositories;

namespace VodovozBusiness.CachingRepositories.Subdivisions
{
	internal sealed class SubdivisionInMemoryTitleCacheRepository : DomainEntityNodeInMemoryCacheRepositoryBase<Subdivision>
	{
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;

		public SubdivisionInMemoryTitleCacheRepository(
			ILogger<IDomainEntityNodeInMemoryCacheRepository<Subdivision>> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<Subdivision> subdivisionRepository)
			: base(logger, unitOfWorkFactory)
		{
			_subdivisionRepository = subdivisionRepository
				?? throw new ArgumentNullException(nameof(subdivisionRepository));
		}

		protected override Subdivision GetEntityById(int id) =>
			_subdivisionRepository
				.Get(
					_unitOfWork,
					x => x.Id == id,
					limit: 1)
				.FirstOrDefault();

		protected override IDictionary<int, string> GetTitlesByIdsFromDatabase(ICollection<int> ids) =>
			_subdivisionRepository
				.GetValue(
					_unitOfWork,
					x => new { x.Id, x.Name },
					x => ids.Contains(x.Id))
				.ToDictionary(x => x.Id, x => x.Name);
	}
}
