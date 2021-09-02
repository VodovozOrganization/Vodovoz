using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.TempAdapters
{
	public class ExpenseCategorySelectorFactory : IExpenseCategorySelectorFactory
	{
		public IEntityAutocompleteSelectorFactory CreateExpenseCategoryAutocompleteSelectorFactory()
		{
			var uow = UnitOfWorkFactory.CreateWithoutRoot($"Фабрика статьи расхода {nameof(ExpenseCategorySelectorFactory)}");

			var expenceCategoryfilterViewModel = new ExpenseCategoryJournalFilterViewModel
			{
				ExcludedIds = new CategoryRepository(new ParametersProvider()).ExpenseSelfDeliveryCategories(uow).Select(x => x.Id),
				HidenByDefault = true
			};
			IFileChooserProvider chooserProvider = new FileChooser("Категории расхода.csv");
			var employeeFilter = new EmployeeFilterViewModel
			{
				Status = EmployeeStatus.IsWorking
			};

			var employeeJournalFactory = new EmployeeJournalFactory(employeeFilter);
			var subdivisionJournalFactory = new SubdivisionJournalFactory();

			return new SimpleEntitySelectorFactory<ExpenseCategory, ExpenseCategoryViewModel>(() =>
			{
				var journal = new SimpleEntityJournalViewModel<ExpenseCategory, ExpenseCategoryViewModel>(
					x => x.Name,
					() => new ExpenseCategoryViewModel(
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						chooserProvider,
						expenceCategoryfilterViewModel,
						employeeJournalFactory,
						subdivisionJournalFactory
					),
					node => new ExpenseCategoryViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						chooserProvider,
						expenceCategoryfilterViewModel,
						employeeJournalFactory,
						subdivisionJournalFactory
					),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices
					);
				journal.SelectionMode = JournalSelectionMode.Single;
				journal.SetFilter(expenceCategoryfilterViewModel,
					filter => Restrictions.Not(Restrictions.In("Id", filter.ExcludedIds.ToArray())));
				return journal;
			});
		}
	}
}
