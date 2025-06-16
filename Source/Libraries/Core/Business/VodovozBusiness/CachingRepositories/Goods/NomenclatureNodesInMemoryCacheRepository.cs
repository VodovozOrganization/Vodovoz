using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Goods;

namespace VodovozBusiness.CachingRepositories.Goods
{
	public sealed class NomenclatureNodesInMemoryCacheRepository
		: DomainEntityNodeInMemoryCacheRepositoryBase<Nomenclature>
	{
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;

		public NomenclatureNodesInMemoryCacheRepository(
			ILogger<NomenclatureNodesInMemoryCacheRepository> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<Nomenclature> nomenclatureRepository)
			: base(logger, unitOfWorkFactory)
		{
			_nomenclatureRepository = nomenclatureRepository
				?? throw new ArgumentNullException(nameof(nomenclatureRepository));
		}

		protected override Nomenclature GetEntityById(int id)
		{
			return _nomenclatureRepository.GetFirstOrDefault(
				_unitOfWork,
				x => x.Id == id);
		}

		protected override IDictionary<int, string> GetTitlesByIdsFromDatabase(ICollection<int> ids) =>
			_nomenclatureRepository
				.Get(_unitOfWork, x => ids.Contains(x.Id))
				.Select(x => (x.Id, x.Name))
				.ToDictionary(x => x.Id, x => x.Name);
	}
}
