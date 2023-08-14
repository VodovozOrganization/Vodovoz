using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.CachingRepositories.Cash
{
	public sealed class FinancialIncomeCategoriesNodesInMemoryCacheRepository
		: DomainEntityNodeInMemoryCacheRepositoryBase<FinancialIncomeCategory>
	{
		private readonly IFinancialIncomeCategoriesRepository _financialIncomeCategoriesRepository;

		public FinancialIncomeCategoriesNodesInMemoryCacheRepository(
			ILogger<FinancialIncomeCategoriesNodesInMemoryCacheRepository> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IFinancialIncomeCategoriesRepository financialIncomeCategoriesRepository)
			: base(logger, unitOfWorkFactory)
		{
			_financialIncomeCategoriesRepository = financialIncomeCategoriesRepository
				?? throw new ArgumentNullException(nameof(financialIncomeCategoriesRepository));
		}

		protected override FinancialIncomeCategory GetEntityById(int id) => _financialIncomeCategoriesRepository
			.Get(_unitOfWork, x => x.Id == id)
			.FirstOrDefault();

		protected override IDictionary<int, string> GetTitlesByIdsFromDatabase(ICollection<int> ids) =>
			_financialIncomeCategoriesRepository
				.Get(_unitOfWork, x => ids.Contains(x.Id))
				.Select(x => (x.Id, x.Title))
				.ToDictionary(x => x.Id, x => x.Title);
	}
}
