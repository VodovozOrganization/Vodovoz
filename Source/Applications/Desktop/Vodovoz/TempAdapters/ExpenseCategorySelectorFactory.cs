using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Cash;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class ExpenseCategorySelectorFactory : IExpenseCategorySelectorFactory
	{
		private readonly IEnumerable<int> _excludedIds;
		private readonly ILifetimeScope _lifetimeScope;

		public ExpenseCategorySelectorFactory(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new System.ArgumentNullException(nameof(lifetimeScope));

			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			using(var uow =
				unitOfWorkFactory.CreateWithoutRoot($"Фабрика статьи расхода {nameof(ExpenseCategorySelectorFactory)}"))
			{
				var categoryRepository = _lifetimeScope.Resolve<ICategoryRepository>();
				_excludedIds = categoryRepository.ExpenseSelfDeliveryCategories(uow).Select(x => x.Id);
			}
		}

		public IEntityAutocompleteSelectorFactory CreateDefaultExpenseCategoryAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<ExpenseCategoryJournalViewModel>(
				typeof(ExpenseCategory),
				() =>
				{
					var expenseCategoryJournalFilterViewModelConfigurationAction = new Action<ExpenseCategoryJournalFilterViewModel>(
						filter =>
						{
							filter.ExcludedIds = _excludedIds;
							filter.HidenByDefault = true;
						});

					var employeeFilterViewModelConfigurationAction = new Action<EmployeeFilterViewModel>(
						filter =>
						{
							filter.Status = EmployeeStatus.IsWorking;
						});

					var expenseCategoryJournalViewModel = _lifetimeScope.Resolve<ExpenseCategoryJournalViewModel>(
						new TypedParameter(typeof(Action<ExpenseCategoryJournalFilterViewModel>),
							expenseCategoryJournalFilterViewModelConfigurationAction),
						new TypedParameter(
							 typeof(Action<EmployeeFilterViewModel>),
							 employeeFilterViewModelConfigurationAction));

					expenseCategoryJournalViewModel.SelectionMode = JournalSelectionMode.Single;

					return expenseCategoryJournalViewModel;
				});
		}
	}
}
