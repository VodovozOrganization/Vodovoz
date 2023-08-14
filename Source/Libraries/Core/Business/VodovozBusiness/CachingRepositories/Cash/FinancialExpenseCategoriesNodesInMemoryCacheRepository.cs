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
	public sealed class FinancialExpenseCategoriesNodesInMemoryCacheRepository
		: DomainEntityNodeInMemoryCacheRepositoryBase<FinancialExpenseCategory>
	{
		private readonly IFinancialExpenseCategoriesRepository _financialExpenseCategoriesRepository;

		public FinancialExpenseCategoriesNodesInMemoryCacheRepository(
			ILogger<FinancialExpenseCategoriesNodesInMemoryCacheRepository> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IFinancialExpenseCategoriesRepository financialExpenseCategoriesRepository)
			: base(logger, unitOfWorkFactory)
		{
			_financialExpenseCategoriesRepository = financialExpenseCategoriesRepository
				?? throw new ArgumentNullException(nameof(financialExpenseCategoriesRepository));
		}

		protected override FinancialExpenseCategory GetEntityById(int id) => _financialExpenseCategoriesRepository
			.Get(_unitOfWork, x => x.Id == id)
			.FirstOrDefault();

		protected override IDictionary<int, string> GetTitlesByIdsFromDatabase(ICollection<int> ids) =>
			_financialExpenseCategoriesRepository
				.Get(_unitOfWork, x => ids.Contains(x.Id))
				.Select(x => (x.Id, x.Title))
				.ToDictionary(x => x.Id, x => x.Title);
	}
}
